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

        [HttpPost("observe")]
        public async Task<IActionResult> Observe([FromBody] CountryListItem item)
        {
            if(item is null || string.IsNullOrWhiteSpace(item.Cca3))
            {
                return BadRequest("Invalid country data.");
            }

            var country = new ObservedCountry();

            try
            {
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

                country.Cca3 = countryData.Cca3;
                country.CountryCommonName = countryData.Name.Common;
                country.CountryOfficialName = countryData.Name.Official;
                country.CountryFlag = countryData.Flags.Svg;
                country.FlagAltText = countryData.Flags.Alt;

                await _countryService.AddObservedCountryAsync(country);
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

            return Ok(country);
        }

        [HttpPut("edit/{cca3}")]
        public async Task<IActionResult> EditObservedCountry(string cca3, [FromBody] CountryListItem updated)
        {
            if(string.IsNullOrWhiteSpace(cca3) || updated is null || string.IsNullOrWhiteSpace(updated.Cca3))
            {
                return BadRequest("Invalid input data.");
            }

            var country = new ObservedCountry();

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

                var response = await _apiService.GetCountryBaseData(updated.Cca3);

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

                country.Id = oldCountry.Id;
                country.Cca3 = countryData.Cca3;
                country.CountryCommonName = countryData.Name.Common;
                country.CountryOfficialName = countryData.Name.Official;
                country.CountryFlag = countryData.Flags.Svg;
                country.FlagAltText = countryData.Flags.Alt;

                await _countryService.EditObservedCountryAsync(country.Id, country);
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

            return Ok(country);
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
            }
            catch(MongoException ex)
            {
                return StatusCode(503, $"Database unavailable: {ex.Message}");
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }

            return NoContent();
        }
    }
}
