using System.Text.Json;
using DetailedCountries.Server.Models;
using DetailedCountries.Server.Models.DTOs;
using DetailedCountries.Server.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace DetailedCountries.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CountryController : Controller
    {
        private readonly ICountryService _countryService;
        private readonly IRESTCountriesAPIService _apiService;
        private readonly IOpenMeteoAPIService _weatherService;

        public CountryController(ICountryService countryService, IRESTCountriesAPIService apiService, IOpenMeteoAPIService weatherService)
        {
            _countryService = countryService;
            _apiService = apiService;
            _weatherService = weatherService;
        }

        [HttpGet("available")]
        public async Task<ActionResult<List<CountryListItem>>> GetAvailableCountries()
        {
            try
            {
                var listItems = await _countryService.GetAllListItemAsync();
                var observed = await _countryService.GetAllObservedCountriesAsync();

                if(listItems is null || listItems.Count == 0)
                {
                    return NotFound("No countries found in the database.");
                }

                var observedCodes = observed?.Select(o => o.Cca3).ToHashSet() ?? new HashSet<string>();
                var countries = listItems.Where(c => !observedCodes.Contains(c.Cca3)).OrderBy(c => c.CountryCommonName).ToList();

                return Ok(countries);
            }
            catch(MongoException ex)
            {
                return StatusCode(503, $"Database unavailable: {ex.Message}");
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        [HttpGet("observed")]
        public async Task<ActionResult<List<ObservedCountry>>> GetObservedCountries()
        {
            try
            {
                var observed = await _countryService.GetAllObservedCountriesAsync();
                var sorted = observed?.OrderBy(c => c.CountryCommonName).ToList() ?? new List<ObservedCountry>() ?? new List<ObservedCountry>();

                return Ok(sorted);
            }
            catch(MongoException ex)
            {
                return StatusCode(503, $"Database unavailable: {ex.Message}");
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        [HttpGet("observed/{cca3}")]
        public async Task<ActionResult<ObservedCountry>> GetObservedCountry(string cca3)
        {
            if(string.IsNullOrWhiteSpace(cca3))
            {
                return BadRequest("Country code is required.");
            }

            try
            {
                var country = await _countryService.GetObservedCountryByCode(cca3);

                if(country is null)
                {
                    return NotFound($"No observed country found with code: {cca3}");
                }

                return Ok(country);
            }
            catch(MongoException ex)
            {
                return StatusCode(503, $"Database unavailable: {ex.Message}");
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        [HttpGet("details/{cca3}")]
        public async Task<IActionResult> GetObservedCountryDetails(string cca3)
        {
            if(string.IsNullOrWhiteSpace(cca3))
            {
                return BadRequest("Country code is required.");
            }

            try
            {
                // Data avaibliity checks & API calls
                var available = await _countryService.GetAllListItemAsync();
                if(available.Find(c => c.Cca3 == cca3) is null)
                {
                    return NotFound($"Country '{cca3}' not found in available countries.");
                }

                var observed = await _countryService.GetAllObservedCountriesAsync();
                var isObserved = observed.Find(c => c.Cca3 == cca3) is not null;
                if(!isObserved)
                {
                    return StatusCode(403, $"Country '{cca3}' is not observed.");
                }

                var response = await _apiService.GetCountryDetailsAsync(cca3);
                if(!response.Success || response.Data.ValueKind != JsonValueKind.Array)
                {
                    return StatusCode(response.StatusCode, $"API error: {response.Message}");
                }

                var countryData = response.Data.Deserialize<List<Country>>();
                if(countryData is null || countryData.Count == 0)
                {
                    return NotFound($"Country '{cca3}' not found in API response.");
                }

                var c = countryData[0];

                // First Native Name
                string? nativeCommon = null, nativeOfficial = null, nativeLang = null;
                if(c.Name?.NativeName?.Count > 0)
                {
                    var first = c.Name.NativeName.First();
                    nativeCommon = first.Value.Common;
                    nativeOfficial = first.Value.Official;

                    if(c.Languages != null && c.Languages.TryGetValue(first.Key, out var lang))
                    {
                        nativeLang = lang;
                    }
                }

                // First Calling Code
                string? callingCode = null;
                if(c.Idd?.Root != null && c.Idd?.Suffixes?.Count > 0)
                {
                    callingCode = c.Idd.Root + c.Idd.Suffixes[0];
                }

                // Coat of Arms Alt Text
                string? coatAlt = null;
                if(c.Demonyms != null && c.Demonyms.TryGetValue("eng", out var demonym))
                {
                    coatAlt = $"{demonym.M} coat of arms";
                }

                // Finance - gini & currency
                string? currencyCode = null, currencyName = null, currencySymbol = null;
                double? gini = null;

                if(c.Gini != null && c.Gini.Count > 0)
                {
                    var first = c.Gini.First();
                    gini = first.Value;
                }

                if(c.Currencies != null && c.Currencies.Count > 0)
                {
                    var first = c.Currencies.First();
                    currencyCode = first.Key;
                    currencyName = first.Value.Name;
                    currencySymbol = first.Value.Symbol;
                }

                // Weather coordinates - capital over country center
                double? capLat = null, capLng = null;
                if(c.CapitalInfo?.Latlng?.Count >= 2)
                {
                    capLat = c.CapitalInfo.Latlng[0];
                    capLng = c.CapitalInfo.Latlng[1];
                }

                double weatherLat = capLat ?? c.Latlng?[0] ?? 0;
                double weatherLng = capLng ?? c.Latlng?[1] ?? 0;
                string weatherLocation = capLat.HasValue ? "capital" : "country center";

                // Weather
                WeatherData? weather = null;
                var weatherResponse = await _weatherService.GetCurrentWeatherAsync(weatherLat, weatherLng);
                if(weatherResponse.Success)
                {
                    var weatherData = weatherResponse.Data;

                    if(weatherData.ValueKind == JsonValueKind.Array)
                    {
                        weatherData = weatherData[0];
                    }

                    if(weatherData.TryGetProperty("current_weather", out var cw))
                    {
                        weather = new WeatherData
                        {
                            Temperature = cw.TryGetProperty("temperature", out var t) ? t.GetDouble() : 0,
                            Windspeed = cw.TryGetProperty("windspeed", out var w) ? w.GetDouble() : 0,
                            WindDirection = cw.TryGetProperty("winddirection", out var wd) ? wd.GetDouble() : 0,
                            IsDay = cw.TryGetProperty("is_day", out var id) && id.GetInt32() == 1,
                            Condition = cw.TryGetProperty("weathercode", out var wc)
                                ? _weatherService.DecodeWeatherCode(wc.GetInt32())
                                : "Unknown",
                            Location = weatherLocation
                        };
                    }
                }

                // Neighbours Data
                var observedCodes = observed.Select(o => o.Cca3).ToHashSet();
                var neighbours = new List<NeighbourData>();
                if(c.Borders != null)
                {
                    foreach(var code in c.Borders)
                    {
                        var isNeighbourObserved = observedCodes.Contains(code);
                        var listItem = available.Find(a => a.Cca3 == code);

                        neighbours.Add(new NeighbourData
                        {
                            Code = code,
                            IsObserved = isNeighbourObserved,
                            ListItem = listItem
                        });
                    }
                }

                // Result DTO
                var result = new CountryDetails
                {
                    CommonName = c.Name?.Common ?? "No data available",
                    OfficialName = c.Name?.Official ?? "No data available",
                    NativeCommonName = nativeCommon,
                    NativeOfficialName = nativeOfficial,
                    NativeLanguage = nativeLang,
                    FlagSvg = c.Flags?.Svg,
                    FlagAlt = c.Flags?.Alt,
                    CoatOfArmsSvg = c.CoatOfArms?.Svg,
                    CoatOfArmsAlt = coatAlt,
                    Latitude = c.Latlng?.Count >= 2 ? c.Latlng[0] : null,
                    Longitude = c.Latlng?.Count >= 2 ? c.Latlng[1] : null,
                    Region = c.Region ?? "No data available",
                    Subregion = c.Subregion ?? "No data available",
                    Area = c.Area > 0 ? $"{c.Area:N0} km²" : "No data available",
                    Landlocked = c.Landlocked,
                    Timezones = c.Timezones ?? new(),
                    Continents = c.Continents ?? new(),
                    Population = c.Population > 0 ? c.Population.ToString("N0") : "No data available",
                    Languages = c.Languages?.Values.ToList() ?? new(),
                    Status = c.Status ?? "No data available",
                    UnMember = c.UnMember,
                    Independent = c.Independent,
                    Cca2 = c.Cca2,
                    Cca3 = c.Cca3,
                    Ccn3 = c.Ccn3,
                    Cioc = c.Cioc,
                    Tld = c.Tld ?? new(),
                    CallingCode = callingCode,
                    Capital = c.Capital?.FirstOrDefault() ?? "No data available",
                    CapitalLat = capLat,
                    CapitalLng = capLng,
                    CurrencyCode = currencyCode,
                    CurrencyName = currencyName,
                    CurrencySymbol = currencySymbol,
                    Gini = gini,
                    Weather = weather,
                    Neighbours = neighbours
                };

                return Ok(result);
            }
            catch (JsonException ex)
            {
                return StatusCode(500, $"Error parsing API response: {ex.Message}");
            }
            catch (MongoException ex)
            {
                return StatusCode(503, $"Database unavailable: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        [HttpPost("observe")]
        public async Task<IActionResult> Observe([FromBody] CountryListItem item)
        {
            if(item is null || string.IsNullOrWhiteSpace(item.Cca3))
            {
                return BadRequest("Invalid country data.");
            }

            try
            {
                var observed = await _countryService.GetAllObservedCountriesAsync();
                bool alreadyExists = observed?.Any(o => o.Cca3 == item.Cca3) ?? false;

                if(alreadyExists)
                {
                    return Conflict($"{item.Cca3} is already in your collection.");
                }

                var response = await _apiService.GetCountryBaseDataAsync(item.Cca3);

                if(!response.Success || response.Data.ValueKind != JsonValueKind.Object)
                {
                    return StatusCode(response.StatusCode, $"API error: {response.Data}, {response.Message}");
                }

                var countryData = response.Data.Deserialize<CountryBaseDataResponse>();

                if(countryData is null || string.IsNullOrWhiteSpace(countryData.Cca3))
                {
                    return NotFound($"Country code: '{item.Cca3}' not found in API response.");
                }

                if(string.IsNullOrEmpty(countryData.Name.Common) || string.IsNullOrWhiteSpace(countryData.Name.Official))
                {
                    return NotFound($"Country name data for '{item.Cca3}' is incomplete in API response.");
                }

                if(string.IsNullOrEmpty(countryData.Flags.Svg) || string.IsNullOrEmpty(countryData.Flags.Alt))
                {
                    return NotFound($"Country flag data for '{item.Cca3}' is incomplete in API response.");
                }

                var country = new ObservedCountry();
                country.Cca3 = countryData.Cca3;
                country.CountryCommonName = countryData.Name.Common;
                country.CountryOfficialName = countryData.Name.Official;
                country.CountryFlag = countryData.Flags.Svg;
                country.FlagAltText = countryData.Flags.Alt;

                await _countryService.AddObservedCountryAsync(country);

                return Ok(country);
            }
            catch(JsonException ex)
            {
                return StatusCode(500, $"Error parsing API response: {ex.Message}");
            }
            catch(MongoException ex)
            {
                return StatusCode(503, $"Database unavailable: {ex.Message}");
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        [HttpPut("edit/{cca3}")]
        public async Task<IActionResult> EditObservedCountry(string cca3, [FromBody] CountryListItem updated)
        {
            if(string.IsNullOrWhiteSpace(cca3) || updated is null || string.IsNullOrWhiteSpace(updated.Cca3))
            {
                return BadRequest("Invalid input data.");
            }

            try
            {
                var observed = await _countryService.GetAllObservedCountriesAsync();
                bool alreadyExists = observed?.Any(o => o.Cca3 == updated.Cca3) ?? false;

                if(alreadyExists)
                {
                    return Conflict($"{updated.Cca3} is already in your collection.");
                }

                var oldCountry = await _countryService.GetObservedCountryByCode(cca3);

                if(oldCountry is null)
                {
                    return NotFound($"No observed country found with code: {cca3}");
                }

                var response = await _apiService.GetCountryBaseDataAsync(updated.Cca3);

                if(!response.Success || response.Data.ValueKind != JsonValueKind.Object)
                {
                    return StatusCode(response.StatusCode, $"API error: {response.Data}, {response.Message}");
                }

                var countryData = response.Data.Deserialize<CountryBaseDataResponse>();

                if(countryData is null || string.IsNullOrWhiteSpace(countryData.Cca3))
                {
                    return NotFound($"Country code: '{updated.Cca3}' not found in API response.");
                }

                if(string.IsNullOrEmpty(countryData.Name.Common) || string.IsNullOrWhiteSpace(countryData.Name.Official))
                {
                    return NotFound($"Country name data for '{updated.Cca3}' is incomplete in API response.");
                }

                if(string.IsNullOrEmpty(countryData.Flags.Svg) || string.IsNullOrEmpty(countryData.Flags.Alt))
                {
                    return NotFound($"Country flag data for '{updated.Cca3}' is incomplete in API response.");
                }

                var country = new ObservedCountry();
                country.Id = oldCountry.Id;
                country.Cca3 = countryData.Cca3;
                country.CountryCommonName = countryData.Name.Common;
                country.CountryOfficialName = countryData.Name.Official;
                country.CountryFlag = countryData.Flags.Svg;
                country.FlagAltText = countryData.Flags.Alt;

                await _countryService.EditObservedCountryAsync(country.Id, country);

                return Ok(country);
            }
            catch(JsonException ex)
            {
                return StatusCode(500, $"Error parsing API response: {ex.Message}");
            }
            catch(MongoException ex)
            {
                return StatusCode(503, $"Database unavailable: {ex.Message}");
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        [HttpDelete("remove/{cca3}")]
        public async Task<IActionResult> RemoveObservedCountry(string cca3)
        {
            if(string.IsNullOrWhiteSpace(cca3))
            {
                return BadRequest("Country code is required.");
            }

            try
            {
                var country = await _countryService.GetObservedCountryByCode(cca3);
                
                if(country is null)
                {
                    return NotFound($"No observed country found with code: {cca3}");
                }

                await _countryService.RemoveObservedCountryAsync(country.Id);

                return NoContent();
            }
            catch(MongoException ex)
            {
                return StatusCode(503, $"Database unavailable: {ex.Message}");
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }
    }
}
