using Quizanchos.DbUpdater.DataSources;
using Quizanchos.DbUpdater.Entities;
using Quizanchos.DbUpdater.Updater;
using Quizanchos.DbUpdater.Utils;

namespace Quizanchos.DbUpdater.SpecificUpdaters;

internal class CurrenciesUpdater
{
    private readonly IDataUpdater _dbUpdater;
    private readonly string _apiKey;

    public CurrenciesUpdater(IDataUpdater dbUpdater, string apiKey)
    {
        _dbUpdater = dbUpdater;
        _apiKey = apiKey;
    }

    public void UpdateSafe()
    {
        try
        {
            Update();
        }
        catch (Exception ex)
        {
            string message = "Could not update currencies\n" + $"Reason: {ex.Message}";
            Console.WriteLine(message);
        }
    }

    private void Update()
    {
        CurrenciesDataSource currenciesDataSource = new CurrenciesDataSource(_apiKey);
        List<Currency> currencies = currenciesDataSource.GetMovies().Result;

        CurrenciesDataToUpdateBuilder moviesDataToUpdateBuilder = new CurrenciesDataToUpdateBuilder();
        DataToUpdate[] dataToUpdate = moviesDataToUpdateBuilder.BuildData(currencies);
        foreach (DataToUpdate data in dataToUpdate)
        {
            _dbUpdater.Update(data).Wait();
        }
    }
}
