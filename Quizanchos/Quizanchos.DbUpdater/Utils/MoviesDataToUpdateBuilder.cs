using Quizanchos.Common.Enums;
using Quizanchos.DbUpdater.Entities;
using Quizanchos.DbUpdater.Updater;

namespace Quizanchos.DbUpdater.Utils;

internal class MoviesDataToUpdateBuilder
{
    public DataToUpdate[] BuildData(List<Movie> movies)
    {
        DataToUpdate[] dataToUpdate =
        [
            BuildCountriesDataToUpdateWithArea(movies),
        ];

        return dataToUpdate;
    }

    private DataToUpdate BuildCountriesDataToUpdateWithArea(List<Movie> movies)
    {
        EntityWithValueToUpdate[] entities = movies.Select(movie => movie.ToUniversalEntityWithRating()).ToArray();
        return new DataToUpdate(FeatureType.Float, "Movie-Rating", "https://static0.srcdn.com/wordpress/wp-content/uploads/2023/11/greatest-movies-of-all-time.jpg",
            "Danchos", DateTime.Now, "Which movies has bigger rating?", isPremium: true, entities);
    }
}
