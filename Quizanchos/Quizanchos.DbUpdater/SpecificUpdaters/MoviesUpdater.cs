using Quizanchos.DbUpdater.DataSources;
using Quizanchos.DbUpdater.Entities;
using Quizanchos.DbUpdater.Updater;
using Quizanchos.DbUpdater.Utils;

namespace Quizanchos.DbUpdater.SpecificUpdaters;

internal class MoviesUpdater
{
    private readonly IDataUpdater _dbUpdater;
    private readonly string _apiKey;
    private readonly int _maxAmountOfMovies;

    public MoviesUpdater(IDataUpdater dbUpdater, string apiKey, int maxAmountOfMovies)
    {
        _dbUpdater = dbUpdater;
        _apiKey = apiKey;
        _maxAmountOfMovies = maxAmountOfMovies;
    }

    public void UpdateSafe()
    {
        try
        {
            Update();
        }
        catch (Exception ex)
        {
            string message = "Could not update countries\n" + $"Reason: {ex.Message}";
            Console.WriteLine(message);
        }
    }

    private void Update()
    {
        MoviesDataSource moviesDataSource = new MoviesDataSource(_apiKey, _maxAmountOfMovies);
        List<Movie> movies = moviesDataSource.GetMovies().Result;

        MoviesDataToUpdateBuilder moviesDataToUpdateBuilder = new MoviesDataToUpdateBuilder();
        DataToUpdate[] dataToUpdate = moviesDataToUpdateBuilder.BuildData(movies);
        foreach (DataToUpdate data in dataToUpdate)
        {
            _dbUpdater.Update(data).Wait();
        }
    }
}
