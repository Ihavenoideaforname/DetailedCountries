using DetailedCountries.Server.Models.BackendModels;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace DetailedCountries.Server.Services
{
    public interface IRESTCountriesAPIService
    {
        Task<APIResult<JsonElement>> GetAvaiableCountriesAsync();
        Task<APIResult<JsonElement>> GetCountryBaseData(string cca3);
    }

    public class RESTCountriesAPIService : IRESTCountriesAPIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RESTCountriesAPIService> _logger;
        private readonly string _apiUrl;

        public RESTCountriesAPIService(HttpClient httpClient, ILogger<RESTCountriesAPIService> logger, IConfiguration configuration) 
        {
            _httpClient = httpClient;
            _logger = logger;

            _apiUrl = configuration["RESTCountriesAPI:Url"] ?? throw new ArgumentNullException("RESTCountriesAPI:Url is not configured.");
        }

        public async Task<APIResult<JsonElement>> GetAvaiableCountriesAsync() =>
            await FetchRESTCountriesAsync($"{_apiUrl}all?fields=name,flags,cca3");

        public async Task<APIResult<JsonElement>> GetCountryBaseData(string cca3) =>
            await FetchRESTCountriesAsync($"{_apiUrl}alpha/{cca3}?fields=name,flags,cca3");

        private async Task<APIResult<JsonElement>> FetchRESTCountriesAsync(string apiUrl)
        {
            try
            {
                _logger.LogInformation("Calling REST Countries API: {Url}", apiUrl);

                var response = await _httpClient.GetAsync(apiUrl);

                if(!response.IsSuccessStatusCode)
                {
                    var message = $"API returned error {(int)response.StatusCode} - {response.ReasonPhrase}";

                    _logger.LogWarning(message);
                    return APIResult<JsonElement>.Fail(message, (int)response.StatusCode);
                }

                var content = await response.Content.ReadAsStringAsync();

                if(string.IsNullOrWhiteSpace(content))
                {
                    const string message = "API returned empty response.";

                    _logger.LogWarning(message);
                    return APIResult<JsonElement>.Fail(message, 204);
                }

                try
                {
                    using var document = JsonDocument.Parse(content);
                    var data = document.RootElement.Clone();

                    _logger.LogInformation("Successfully fetched countries data.");
                    return APIResult<JsonElement>.Ok(data, (int)response.StatusCode);
                }
                catch(JsonException ex)
                {
                    _logger.LogError(ex, "JSON parsing failed.");
                    return APIResult<JsonElement>.Fail("Invalid JSON received from API.", 500);
                }
            }
            catch(HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed.");
                return APIResult<JsonElement>.Fail("Error while calling external API.", 503);
            }
            catch(TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout.");
                return APIResult<JsonElement>.Fail("Request to external API timed out.", 504);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unexpected error.");
                return APIResult<JsonElement>.Fail("Unexpected error occurred.", 500);
            }
        }
    }
}
