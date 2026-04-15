using System.Transactions;
using System.Text.Json.Serialization;

namespace DetailedCountries.Server.Models.DTOs
{
    public class Country
    {
        // Codes

        [JsonPropertyName("tld")]
        public List<string>? Tld { get; set; }

        [JsonPropertyName("cca2")]
        public string? Cca2 { get; set; }

        [JsonPropertyName("ccn3")]
        public string? Ccn3 { get; set; }

        [JsonPropertyName("cca3")]
        public string? Cca3 { get; set; }

        [JsonPropertyName("cioc")]
        public string? Cioc { get; set; }

        // Status

        [JsonPropertyName("independent")]
        public bool Independent { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("unMember")]
        public bool UnMember { get; set; }

        [JsonPropertyName("idd")]
        public Idd? Idd { get; set; }

        // Geography

        [JsonPropertyName("capital")]
        public List<string>? Capital { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        [JsonPropertyName("subregion")]
        public string? Subregion { get; set; }

        [JsonPropertyName("landlocked")]
        public bool Landlocked { get; set; }

        [JsonPropertyName("borders")]
        public List<string>? Borders { get; set; }

        [JsonPropertyName("area")]
        public double Area { get; set; }

        [JsonPropertyName("population")]
        public long Population { get; set; }

        [JsonPropertyName("timezones")]
        public List<string>? Timezones { get; set; }

        [JsonPropertyName("continents")]
        public List<string>? Continents { get; set; }

        // Names

        [JsonPropertyName("name")]
        public FullName? Name { get; set; }

        [JsonPropertyName("latlng")]
        public List<double>? Latlng { get; set; }

        [JsonPropertyName("demonyms")]
        public Dictionary<string, Demonym>? Demonyms { get; set; }

        [JsonPropertyName("currencies")]
        public Dictionary<string, Currency>? Currencies { get; set; }

        [JsonPropertyName("languages")]
        public Dictionary<string, string>? Languages { get; set; }

        [JsonPropertyName("gini")]
        public Dictionary<string, double>? Gini { get; set; }

        [JsonPropertyName("flags")]
        public Flags? Flags { get; set; }

        [JsonPropertyName("coatOfArms")]
        public CoatOfArms? CoatOfArms { get; set; }

        [JsonPropertyName("capitalInfo")]
        public CapitalInfo? CapitalInfo { get; set; }
    }

    public class Idd
    {
        [JsonPropertyName("root")]
        public string? Root { get; set; }

        [JsonPropertyName("suffixes")]
        public List<string>? Suffixes { get; set; }
    }

    public class FullName
    {
        [JsonPropertyName("common")]
        public string? Common { get; set; }

        [JsonPropertyName("official")]
        public string? Official { get; set; }

        [JsonPropertyName("nativeName")]
        public Dictionary<string, NativeName>? NativeName { get; set; }
    }

    public class NativeName
    {
        [JsonPropertyName("official")]
        public string? Official { get; set; }

        [JsonPropertyName("common")]
        public string? Common { get; set; }
    }

    public class Currency
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("symbol")]
        public string? Symbol { get; set; }
    }

    public class CoatOfArms
    {
        [JsonPropertyName("svg")]
        public string? Svg { get; set; }
    }

    public class CapitalInfo
    {
        [JsonPropertyName("latlng")]
        public List<double>? Latlng { get; set; }
    }

    public class Demonym
    {
        [JsonPropertyName("f")]
        public string? F { get; set; }

        [JsonPropertyName("m")]
        public string? M { get; set; }
    }
}
