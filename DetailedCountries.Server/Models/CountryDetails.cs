using System.Text.Json.Serialization;

namespace DetailedCountries.Server.Models
{
    public class CountryDetails
    {
        [JsonPropertyName("commonName")]
        public string CommonName { get; set; } = "No data available";

        [JsonPropertyName("officialName")]
        public string OfficialName { get; set; } = "No data available";

        [JsonPropertyName("nativeCommonName")]
        public string? NativeCommonName { get; set; }

        [JsonPropertyName("nativeOfficialName")]
        public string? NativeOfficialName { get; set; }

        [JsonPropertyName("nativeLanguage")]
        public string? NativeLanguage { get; set; }

        [JsonPropertyName("flagSvg")]
        public string? FlagSvg { get; set; }

        [JsonPropertyName("flagAlt")]
        public string? FlagAlt { get; set; }

        [JsonPropertyName("coatOfArmsSvg")]
        public string? CoatOfArmsSvg { get; set; }

        [JsonPropertyName("coatOfArmsAlt")]
        public string? CoatOfArmsAlt { get; set; }

        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        // Geography

        [JsonPropertyName("region")]
        public string Region { get; set; } = "No data available";

        [JsonPropertyName("subregion")]
        public string Subregion { get; set; } = "No data available";

        [JsonPropertyName("area")]
        public string Area { get; set; } = "No data available";

        [JsonPropertyName("landlocked")]
        public bool Landlocked { get; set; }

        [JsonPropertyName("timezones")]
        public List<string> Timezones { get; set; } = new();

        [JsonPropertyName("continents")]
        public List<string> Continents { get; set; } = new();

        // Demographics

        [JsonPropertyName("population")]
        public string Population { get; set; } = "No data available";

        [JsonPropertyName("languages")]
        public List<string> Languages { get; set; } = new();

        // Political

        [JsonPropertyName("status")]
        public string Status { get; set; } = "No data available";

        [JsonPropertyName("unMember")]
        public bool UnMember { get; set; }

        [JsonPropertyName("independent")]
        public bool Independent { get; set; }

        // Codes

        [JsonPropertyName("cca2")]
        public string? Cca2 { get; set; }

        [JsonPropertyName("cca3")]
        public string? Cca3 { get; set; }

        [JsonPropertyName("ccn3")]
        public string? Ccn3 { get; set; }

        [JsonPropertyName("cioc")]
        public string? Cioc { get; set; }

        [JsonPropertyName("tld")]
        public List<string> Tld { get; set; } = new();

        [JsonPropertyName("callingCode")]
        public string? CallingCode { get; set; }

        // Capital

        [JsonPropertyName("capital")]
        public string Capital { get; set; } = "No data available";

        [JsonPropertyName("capitalLat")]
        public double? CapitalLat { get; set; }

        [JsonPropertyName("capitalLng")]
        public double? CapitalLng { get; set; }

        // Finance

        [JsonPropertyName("currencyCode")]
        public string? CurrencyCode { get; set; }

        [JsonPropertyName("currencyName")]
        public string? CurrencyName { get; set; }

        [JsonPropertyName("currencySymbol")]
        public string? CurrencySymbol { get; set; }

        [JsonPropertyName("gini")]
        public double? Gini { get; set; }

        // Weather

        [JsonPropertyName("weather")]
        public WeatherData? Weather { get; set; }

        // Neighbours

        [JsonPropertyName("neighbours")]
        public List<NeighbourData> Neighbours { get; set; } = new();
    }

    public class WeatherData
    {

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("windspeed")]
        public double Windspeed { get; set; }

        [JsonPropertyName("windDirection")]
        public double WindDirection { get; set; }

        [JsonPropertyName("isDay")]
        public bool IsDay { get; set; }

        [JsonPropertyName("condition")]
        public string Condition { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;
    }

    public class NeighbourData
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("isObserved")]
        public bool IsObserved { get; set; }

        [JsonPropertyName("listItem")]
        public CountryListItem? ListItem { get; set; }
    }
}
