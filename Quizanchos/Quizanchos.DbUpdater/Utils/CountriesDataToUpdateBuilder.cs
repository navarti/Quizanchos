using Quizanchos.Common.Enums;
using Quizanchos.DbUpdater.Entities;
using Quizanchos.DbUpdater.Updater;

namespace Quizanchos.DbUpdater.Utils;

internal class CountriesDataToUpdateBuilder
{
    public DataToUpdate[] BuildData(List<Country> countries)
    {
        DataToUpdate[] dataToUpdate =
        [
            BuildCountriesDataToUpdateWithArea(countries),
            BuildCountriesDataToUpdateWithPopulation(countries),
        ];

        return dataToUpdate;
    }

    private DataToUpdate BuildCountriesDataToUpdateWithArea(List<Country> countries)
    {
        EntityWithValueToUpdate[] entities = countries.Select(country => country.ToUniversalEntityWithArea()).ToArray();
        return new DataToUpdate(FeatureType.Float, "Country-Area", entities);
    }

    private DataToUpdate BuildCountriesDataToUpdateWithPopulation(List<Country> countries)
    {
        EntityWithValueToUpdate[] entities = countries.Select(country => country.ToUniversalEntityWithPopulation()).ToArray();
        return new DataToUpdate(FeatureType.Int, "Country-Population", entities);
    }
}
