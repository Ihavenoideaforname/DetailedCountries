using DetailedCountries.Server.Models;
using DetailedCountries.Server.Models.BackendModels;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace DetailedCountries.Server.Services
{
    public interface ICountryService
    {
        // CountryListItem collection methods
        Task<List<CountryListItem>> GetAllListItemAsync();
        Task BulkWriteListItemAsync(List<WriteModel<CountryListItem>> writes);

        // ObservedCountry collection methods
        Task<List<ObservedCountry>> GetAllObservedCountriesAsync();
        Task AddObservedCountryAsync(ObservedCountry country);
    }

    public class CountryService : ICountryService
    {
        private readonly IMongoCollection<CountryListItem> _countryListItemCollection;
        private readonly IMongoCollection<ObservedCountry> _observedCountryCollection;

        public CountryService(IOptions<DetailedCountriesDBSettings> detailedCountriesDBSettings)
        {
            var mongoClient = new MongoClient(detailedCountriesDBSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(detailedCountriesDBSettings.Value.DatabaseName);

            _countryListItemCollection = mongoDatabase.GetCollection<CountryListItem>(detailedCountriesDBSettings.Value.CountryListItemsCollectionName);
            _observedCountryCollection = mongoDatabase.GetCollection<ObservedCountry>(detailedCountriesDBSettings.Value.ObservedCountriesCollectionName);
        }

        //CountryListItem collection methods

        public async Task<List<CountryListItem>> GetAllListItemAsync() =>
            await _countryListItemCollection.Find(_ => true).ToListAsync();

        public async Task BulkWriteListItemAsync(List<WriteModel<CountryListItem>> writes) =>
            await _countryListItemCollection.BulkWriteAsync(writes);


        //ObservedCountry collection methods

        public async Task<List<ObservedCountry>> GetAllObservedCountriesAsync() =>
            await _observedCountryCollection.Find(_ => true).ToListAsync();

        public async Task AddObservedCountryAsync(ObservedCountry country) =>
            await _observedCountryCollection.InsertOneAsync(country);
    }
}
