using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace DetailedCountries.Server.Models
{
    public class ObservedCountry : CountryListItem
    {
        [BsonElement("OfficialName")]
        [JsonPropertyName("OfficialName")]
        public string CountryOfficialName { get; set; } = null!;
    }
}
