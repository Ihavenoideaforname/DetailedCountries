using System.Text.Json.Serialization;

namespace DetailedCountries.Server.Models.DTOs
{
    public class CountryListResponse
    {
        [JsonPropertyName("cca3")]
        public string Cca3 { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public Name Name { get; set; } = new Name();

        [JsonPropertyName("flags")]
        public Flags Flags { get; set; } = new Flags();
    }

    public class Name
    {
        [JsonPropertyName("common")]
        public string Common { get; set; } = string.Empty;
    }

    public class Flags
    {
        [JsonPropertyName("svg")]
        public string Svg { get; set; } = string.Empty;

        [JsonPropertyName("alt")]
        public string Alt { get; set; } = string.Empty;
    }
}
