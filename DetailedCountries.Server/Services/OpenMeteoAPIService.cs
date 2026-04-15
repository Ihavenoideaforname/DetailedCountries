using DetailedCountries.Server.Models.BackendModels;
using System.Globalization;
using System.Text.Json;

namespace DetailedCountries.Server.Services
{
    public interface IOpenMeteoAPIService
    {
        Task<APIResult<JsonElement>> GetCurrentWeatherAsync(double lat, double lng);
        string DecodeWeatherCode(int code);
    }

    public class OpenMeteoAPIService : IOpenMeteoAPIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RESTCountriesAPIService> _logger;
        private readonly string _apiUrl;

        public OpenMeteoAPIService(HttpClient httpClient, ILogger<RESTCountriesAPIService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;

            _apiUrl = configuration["OpenMeteoAPI:Url"] ?? throw new ArgumentNullException("OpenMeteoAPI:Url is not configured.");
        }

        public async Task<APIResult<JsonElement>> GetCurrentWeatherAsync(double lat, double lng) =>
            await FetchOpenMeteoAsync($"{_apiUrl}?latitude={lat.ToString(CultureInfo.InvariantCulture)}&longitude={lng.ToString(CultureInfo.InvariantCulture)}&current_weather=true");

        public string DecodeWeatherCode(int code)
        {
            return code switch
            {
                0 => "Clear sky",
                1 => "Mainly clear",
                2 => "Partly cloudy",
                3 => "Overcast",
                45 => "Fog",
                48 => "Depositing rime fog",
                51 => "Light drizzle",
                53 => "Moderate drizzle",
                55 => "Dense intensity drizzle",
                56 => "Light freezing drizzle",
                57 => "Dense intensity freezing drizzle",
                61 => "Slight rain",
                63 => "Moderate rain",
                65 => "Heavy intensity rain",
                66 => "Light freezing rain",
                67 => "Heavy intensity freezing rain",
                71 => "Slight snow fall",
                73 => "Moderate snow fall",
                75 => "Heavy intensity snow fall",
                77 => "Snow grains",
                80 => "Slight rain showers",
                81 => "Moderate rain showers",
                82 => "Violent rain showers",
                85 => "Slight snow showers",
                86 => "Heavy snow showers",
                95 => "Slight/Moderate thunderstorm",
                96 => "Thunderstorm with slight hail",
                99 => "Thunderstorm with heavy hail",
                _ => "Unknown weather code"
            };
        }

        private async Task<APIResult<JsonElement>> FetchOpenMeteoAsync(string apiUrl)
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
