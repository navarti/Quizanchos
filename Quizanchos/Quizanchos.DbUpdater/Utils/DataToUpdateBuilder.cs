using Quizanchos.DbUpdater.Entities;
using Quizanchos.DbUpdater.Updater;

namespace Quizanchos.DbUpdater.Utils;

internal static class DataToUpdateBuilder
{
    public static DataToUpdate<float> BuildCountriesDataToUpdateWithArea(List<Country> countries)
    {
        DataToUpdate<float>.EntityWithValueToUpdate[] entities = countries.Select(country => country.ToUniversalEntityWithArea()).ToArray();
        return new DataToUpdate<float>("Country-Area", entities);
    }

    public static DataToUpdate<int> BuildCountriesDataToUpdateWithPopulation(List<Country> countries)
    {
        DataToUpdate<int>.EntityWithValueToUpdate[] entities = countries.Select(country => country.ToUniversalEntityWithPopulation()).ToArray();
        return new DataToUpdate<int>("Country-Population", entities);
    }
}

