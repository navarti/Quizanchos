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
        return new DataToUpdate(FeatureType.Float, "Country-Area", "https://cdn.britannica.com/96/212396-050-8B527272/world-map-with-country-borders.jpg", 
            "Danchos", DateTime.Now, "Which country has bigger area?", isPremium: false, entities);
    }

    private DataToUpdate BuildCountriesDataToUpdateWithPopulation(List<Country> countries)
    {
        EntityWithValueToUpdate[] entities = countries.Select(country => country.ToUniversalEntityWithPopulation()).ToArray();
        return new DataToUpdate(FeatureType.Int, "Country-Population", "https://www.healthstaffrecruitment.com.au/wp-content/uploads/2018/07/bigstock-World-population-rise-and-Eart-13736474.jpg", 
            "Vanchos", DateTime.Now, "Which country has more population?", isPremium: false, entities);
    }
}
