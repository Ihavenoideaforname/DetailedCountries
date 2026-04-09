using DetailedCountries.Server.Models;
using DetailedCountries.Server.Models.DTOs;
using MongoDB.Driver;
using System.Text.Json;

namespace DetailedCountries.Server.Services
{
    public class CountrySyncService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpClient _httpClient;
        private readonly ILogger<CountrySyncService> _logger;

        public CountrySyncService(IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory, ILogger<CountrySyncService> logger)
        {
            _serviceProvider = serviceProvider;
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await SyncData(stoppingToken);

            while(!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                await SyncData(stoppingToken);
            }
        }

        private async Task SyncData(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Starting country sync...");

                // Get DB and API services

                using var scope = _serviceProvider.CreateScope();

                var apiService = scope.ServiceProvider.GetRequiredService<IRESTCountriesAPIService>();
                var countryService = scope.ServiceProvider.GetRequiredService<ICountryService>();

                // Get API data

                var response = await apiService.GetAvaiableCountriesAsync();
                
                if(!response.Success)
                {
                    _logger.LogWarning("API call failed: {Message}", response.Message);
                    return;
                }

                if(response.Data.ValueKind != JsonValueKind.Array)
                {
                    _logger.LogWarning("API returned unexpected data format.");
                    return;
                }

                var apiData = response.Data;

                if(apiData.GetArrayLength() == 0)
                {
                    _logger.LogWarning("API returned empty data.");
                    return;
                }

                // Sync DB with API data

                var dbCountries = await countryService.GetAllListItemAsync();
                var dbDict = dbCountries.ToDictionary(x => x.Cca3);

                var writes = new List<WriteModel<CountryListItem>>();
                var apiCodes = new HashSet<string>();

                int inserted = 0, updated = 0;

                foreach(var api in apiData.EnumerateArray())
                {
                    var countryItem = api.Deserialize<CountryListResponse>();

                    if(countryItem is null)
                    {
                        _logger.LogWarning("Skipping entry with invalid data format.");
                        continue;
                    }

                    if(string.IsNullOrWhiteSpace(countryItem.Cca3))
                    {
                        _logger.LogWarning("Skipping entry with empty cca3 code.");
                        continue;
                    }

                    if(string.IsNullOrEmpty(countryItem.Name.Common) || string.IsNullOrEmpty(countryItem.Flags.Svg) || string.IsNullOrEmpty(countryItem.Flags.Alt))
                    {
                        _logger.LogWarning("Skipping entry with missing name or flag for cca3: {Cca3}", countryItem.Cca3);
                        continue;
                    }

                    apiCodes.Add(countryItem.Cca3);

                    if(!dbDict.TryGetValue(countryItem.Cca3, out var existing))
                    {
                        writes.Add(new InsertOneModel<CountryListItem>(
                            new CountryListItem
                            {
                                Cca3 = countryItem.Cca3,
                                CountryCommonName = countryItem.Name.Common,
                                CountryFlag = countryItem.Flags.Svg,
                                FlagAltText = countryItem.Flags.Alt
                            }));

                        inserted++;
                    }
                    else if(existing.CountryCommonName != countryItem.Name.Common || existing.CountryFlag != countryItem.Flags.Svg || existing.FlagAltText != countryItem.Flags.Alt)
                    {
                        writes.Add(new ReplaceOneModel<CountryListItem>(
                            Builders<CountryListItem>.Filter.Eq(x => x.Id, existing.Id),
                            new CountryListItem
                            {
                                Id = existing.Id,
                                Cca3 = countryItem.Cca3,
                                CountryCommonName = countryItem.Name.Common,
                                CountryFlag = countryItem.Flags.Svg,
                                FlagAltText = countryItem.Flags.Alt
                            }
                        ));

                        updated++;
                    }
                }

                var toDeleteIds = dbCountries
                    .Where(db => !apiCodes.Contains(db.Cca3))
                    .Select(db => db.Id)
                    .ToList();

                int deleted = toDeleteIds.Count;

                if(toDeleteIds.Any())
                {
                    writes.Add(new DeleteManyModel<CountryListItem>(
                        Builders<CountryListItem>.Filter.In(x => x.Id, toDeleteIds)
                    ));
                }

                if(writes.Any())
                {
                    await countryService.BulkWriteListItemAsync(writes);
                }

                _logger.LogInformation($"Country sync finished: +{inserted} ~{updated} -{deleted}");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error during country sync");
            }
        }
    }
}
