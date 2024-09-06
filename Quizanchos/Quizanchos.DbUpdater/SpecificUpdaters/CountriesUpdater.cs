using Quizanchos.DbUpdater.DataSources;
using Quizanchos.DbUpdater.Entities;
using Quizanchos.DbUpdater.Updater;
using Quizanchos.DbUpdater.Utils;

namespace Quizanchos.DbUpdater.SpecificUpdaters;

internal class CountriesUpdater
{
    IDataUpdater _dbUpdater;

    public CountriesUpdater(IDataUpdater dbUpdater)
    {
        _dbUpdater = dbUpdater;
    }

    public void Update()
    {
        CountryDataSource cityDataSource = new CountryDataSource();

        List<Country> cities = cityDataSource.GetCountriesSafe();

        if (cityDataSource.Exceptions.Count > 0)
        {
            foreach (var exception in cityDataSource.Exceptions)
            {
                Console.WriteLine(exception.Message);
                return;
            }
        }

        DataToUpdate<float> dataToUpdateWithArea = DataToUpdateBuilder.BuildCountriesDataToUpdateWithArea(cities);
        _dbUpdater.Update(dataToUpdateWithArea).Wait();
        
        DataToUpdate<int> dataToUpdateWithPopulation = DataToUpdateBuilder.BuildCountriesDataToUpdateWithPopulation(cities);
        _dbUpdater.Update(dataToUpdateWithPopulation).Wait();
    }
}
