namespace Quizanchos.DbUpdater.Updater;

internal class DataToUpdate
{
    public Type FeatureType { get; }
    public string CategoryName { get; }
    public EntityWithValueToUpdate[] Entities { get; }

    public DataToUpdate(Type featureType, string categoryName, EntityWithValueToUpdate[] entities)
    {
        FeatureType = featureType;
        CategoryName = categoryName;
        Entities = entities;
    }
}
