namespace DetailedCountries.Server.Models.BackendModels
{
    public class DetailedCountriesDBSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string CountryListItemsCollectionName { get; set; } = null!;
        public string ObservedCountriesCollectionName { get; set; } = null!;
    }
}
