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

    public DataToUpdate<float>.EntityWithValueToUpdate ToUniversalEntityWithArea()
    {
        return new DataToUpdate<float>.EntityWithValueToUpdate(Name, Area);
    }

    public DataToUpdate<int>.EntityWithValueToUpdate ToUniversalEntityWithPopulation()
    {
        return new DataToUpdate<int>.EntityWithValueToUpdate(Name, Population);
    }
}
