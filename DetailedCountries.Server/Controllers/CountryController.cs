using System.Text.Json;
using DetailedCountries.Server.Models;
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

        public CountryController(ICountryService countryService, IRESTCountriesAPIService apiService)
        {
            _countryService = countryService;
            _apiService = apiService;
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

        [HttpPost("observe")]
        public async Task<IActionResult> Observe([FromBody] CountryListItem item)
        {
            if(item is null || string.IsNullOrWhiteSpace(item.Cca3))
            {
                return BadRequest("Invalid country data.");
            }

            var observed = await _countryService.GetAllObservedCountriesAsync();
            bool alreadyExists = observed?.Any(o => o.Cca3 == item.Cca3) ?? false;

            if(alreadyExists)
            {
                return Conflict($"{item.Cca3} is already in your collection.");
            }

            var response = await _apiService.GetCountryBaseData(item.Cca3);

            if(!response.Success || response.Data.ValueKind != JsonValueKind.Object)
            {
                return StatusCode(response.StatusCode, $"API error: {response.Data}, {response.Message}");
            }

            var apiData = response.Data;

            if(!apiData.TryGetProperty("cca3", out var cca3Prop))
            {
                return NotFound($"Country code: '{item.Cca3}' not found in API response.");
            }

            var cca3 = cca3Prop.GetString();

            if(string.IsNullOrWhiteSpace(cca3))
            {
                return NotFound($"Country code: '{item.Cca3}' not found in API response.");
            }

            if(!apiData.TryGetProperty("name", out var nameProp) || !nameProp.TryGetProperty("common", out var commonNameProp) || !nameProp.TryGetProperty("official", out var officialNameProp))
            {
                return NotFound($"Country name data for '{item.Cca3}' is incomplete in API response.");
            }

            var commonName = commonNameProp.GetString();
            var officialName = officialNameProp.GetString();

            if(string.IsNullOrWhiteSpace(commonName) || string.IsNullOrWhiteSpace(officialName))
            {
                return NotFound($"Country name data for '{item.Cca3}' is incomplete in API response.");
            }

            if(!apiData.TryGetProperty("flags", out var flagsProp) || !flagsProp.TryGetProperty("svg", out var flagProp) || !flagsProp.TryGetProperty("alt", out var altProp))
            {
                return NotFound($"Country flag data for '{item.Cca3}' is incomplete in API response.");
            }

            var flag = flagProp.GetString();
            var altText = altProp.GetString();

            if(string.IsNullOrWhiteSpace(flag) || string.IsNullOrWhiteSpace(altText))
            {
                return NotFound($"Country flag data for '{item.Cca3}' is incomplete in API response.");
            }

            var country = new ObservedCountry
            {
                Cca3 = cca3,
                CountryCommonName = commonName,
                CountryOfficialName = officialName,
                CountryFlag = flag,
                FlagAltText = altText
            };

            await _countryService.AddObservedCountryAsync(country);

            return Ok(country);
        }
    }
}
