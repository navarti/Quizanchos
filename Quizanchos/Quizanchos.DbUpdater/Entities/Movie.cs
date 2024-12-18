using Quizanchos.Common.FeatureTypes;
using Quizanchos.DbUpdater.Updater;

namespace Quizanchos.DbUpdater.Entities;

internal class Movie
{
    public string Name { get; set; }
    public float Rating { get; set; }

    public Movie(string name, float rating)
    {
        Name = name;
        Rating = rating;
    }

    public EntityWithValueToUpdate ToUniversalEntityWithRating()
    {
        FeatureValueFloat featureValueFloat = new FeatureValueFloat(Rating);
        return new EntityWithValueToUpdate(Name, featureValueFloat);
    }
}
