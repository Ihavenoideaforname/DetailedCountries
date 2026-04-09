using System.Text.Json.Serialization;

namespace DetailedCountries.Server.Models.DTOs
{
    public class CountryBaseDataResponse
    {
        [JsonPropertyName("cca3")]
        public string Cca3 { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public Names Name { get; set; } = new Names();

        [JsonPropertyName("flags")]
        public Flags Flags { get; set; } = new Flags();
    }

    public class Names : Name
    {
        [JsonPropertyName("official")]
        public string Official { get; set; } = string.Empty;
    }
}
