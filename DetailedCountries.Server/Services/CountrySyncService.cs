using DetailedCountries.Server.Models;
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
                    if(!api.TryGetProperty("cca3", out var cca3Prop))
                    {
                        _logger.LogWarning("Skipping entry without cca3 code.");
                        continue;
                    }

                    var cca3 = cca3Prop.GetString();

                    if(string.IsNullOrWhiteSpace(cca3))
                    {
                        _logger.LogWarning("Skipping entry with empty cca3 code.");
                        continue;
                    }

                    apiCodes.Add(cca3);

                    var commonName = api.GetProperty("name").GetProperty("common").GetString();
                    var flag = api.GetProperty("flags").GetProperty("svg").GetString();
                    var flagAlt = api.GetProperty("flags").GetProperty("alt").GetString();

                    if(commonName is null || flag is null || flagAlt is null)
                    {
                        _logger.LogWarning("Skipping entry with missing name or flag for cca3: {Cca3}", cca3);
                        continue;
                    }

                    if(!dbDict.TryGetValue(cca3, out var existing))
                    {
                        writes.Add(new InsertOneModel<CountryListItem>(
                            new CountryListItem
                            {
                                Cca3 = cca3,
                                CountryCommonName = commonName,
                                CountryFlag = flag,
                                FlagAltText = flagAlt
                            }));

                        inserted++;
                    }
                    else if(existing.CountryCommonName != commonName || existing.CountryFlag != flag || existing.FlagAltText != flagAlt)
                    {
                        writes.Add(new ReplaceOneModel<CountryListItem>(
                            Builders<CountryListItem>.Filter.Eq(x => x.Id, existing.Id),
                            new CountryListItem
                            {
                                Id = existing.Id,
                                Cca3 = cca3,
                                CountryCommonName = commonName,
                                CountryFlag = flag,
                                FlagAltText = flagAlt
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
