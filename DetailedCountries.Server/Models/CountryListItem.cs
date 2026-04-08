using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DetailedCountries.Server.Models
{
    public class CountryListItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("Code")]
        [JsonPropertyName("Code")]
        public string Cca3 { get; set; } = null!;

        [BsonElement("Name")]
        [JsonPropertyName("Name")]
        public string CountryCommonName { get; set; } = null!;

        [BsonElement("Flag")]
        [JsonPropertyName("Flag")]
        public string CountryFlag { get; set; } = null!;

        [BsonElement("Alt")]
        [JsonPropertyName("Alt")]
        public string FlagAltText { get; set; } = null!;
    }
}
