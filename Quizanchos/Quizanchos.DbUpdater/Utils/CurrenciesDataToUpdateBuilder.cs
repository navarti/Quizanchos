using Quizanchos.Common.Enums;
using Quizanchos.DbUpdater.Entities;
using Quizanchos.DbUpdater.Updater;

namespace Quizanchos.DbUpdater.Utils;

internal class CurrenciesDataToUpdateBuilder
{
    public DataToUpdate[] BuildData(List<Currency> currencies)
    {
        DataToUpdate[] dataToUpdate =
        [
            BuildCurrenciesDataToUpdateWithRate(currencies),
        ];

        return dataToUpdate;
    }

    private DataToUpdate BuildCurrenciesDataToUpdateWithRate(List<Currency> currencies)
    {
        EntityWithValueToUpdate[] entities = currencies.Select(currency => currency.ToUniversalEntityWithRate()).ToArray();
        return new DataToUpdate(FeatureType.Float, "Currencies-Rate", "https://youmatter.world/app/uploads/2021/04/local-currencies-alternative-economy2.jpg",
            "Danchos", DateTime.Now, "Which currencies has bigger ratio to USD?", isPremium: true, entities);
    }
}
