using Quizanchos.Common.FeatureTypes;
using Quizanchos.DbUpdater.Updater;

namespace Quizanchos.DbUpdater.Entities;

internal class Country
{
    public string Name { get; }
    public float Area { get; }
    public int Population { get; }

    public Country(string name, float area, int population)
    {
        Name = name;
        Area = area;
        Population = population;
    }

    public EntityWithValueToUpdate ToUniversalEntityWithArea()
    {
        FeatureValueFloat featureValueFloat = new FeatureValueFloat(Area);
        return new EntityWithValueToUpdate(Name, featureValueFloat);
    }

    public EntityWithValueToUpdate ToUniversalEntityWithPopulation()
    {
        FeatureValueInt featureValueInt = new FeatureValueInt(Population);
        return new EntityWithValueToUpdate(Name, featureValueInt);
    }
}
