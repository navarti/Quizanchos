using Quizanchos.DbUpdater.DataSources;
using Quizanchos.DbUpdater.Entities;
using Quizanchos.DbUpdater.Updater;
using Quizanchos.DbUpdater.Utils;

namespace Quizanchos.DbUpdater.SpecificUpdaters;

internal class CountriesUpdater
{
    private readonly IDataUpdater _dbUpdater;

    public CountriesUpdater(IDataUpdater dbUpdater)
    {
        _dbUpdater = dbUpdater;
    }

    public void UpdateSafe()
    {
        try
        {
            Update();
        }
        catch(Exception ex)
        {
            string message = "Could not update countries\n" + $"Reason: {ex.Message}";
            Console.WriteLine(message);
        }
    }

    private void Update()
    {
        CountryDataSource countryDataSource = new CountryDataSource();
        List<Country> coutries = countryDataSource.GetCountries().Result;

        CountriesDataToUpdateBuilder countriesDataToUpdateBuilder = new CountriesDataToUpdateBuilder();
        DataToUpdate[] dataToUpdate = countriesDataToUpdateBuilder.BuildData(coutries);
        foreach(DataToUpdate data in dataToUpdate)
        {
            _dbUpdater.Update(data).Wait();
        }
    }
}
