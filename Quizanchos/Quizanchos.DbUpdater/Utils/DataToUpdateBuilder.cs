using Quizanchos.Common.FeatureTypes;
using Quizanchos.DbUpdater.Entities;
using Quizanchos.DbUpdater.Updater;

namespace Quizanchos.DbUpdater.Utils;

internal static class DataToUpdateBuilder
{
    public static DataToUpdate BuildCountriesDataToUpdateWithArea(List<Country> countries)
    {
        EntityWithValueToUpdate[] entities = countries.Select(country => country.ToUniversalEntityWithArea()).ToArray();
        return new DataToUpdate(typeof(FeatureValueFloat), "Country-Area", entities);
    }

    public static DataToUpdate BuildCountriesDataToUpdateWithPopulation(List<Country> countries)
    {
        EntityWithValueToUpdate[] entities = countries.Select(country => country.ToUniversalEntityWithPopulation()).ToArray();
        return new DataToUpdate(typeof(FeatureValueInt), "Country-Population", entities);
    }
}
