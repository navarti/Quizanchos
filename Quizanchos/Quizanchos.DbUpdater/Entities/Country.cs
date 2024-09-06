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

    public EntityWithValueToUpdate<float> ToUniversalEntityWithArea()
    {
        return new EntityWithValueToUpdate<float>(Name, Area);
    }

    public EntityWithValueToUpdate<int> ToUniversalEntityWithPopulation()
    {
        return new EntityWithValueToUpdate<int>(Name, Population);
    }
}
