using Quizanchos.Common.Enums;

namespace Quizanchos.DbUpdater.Updater;

internal class DataToUpdate
{
    public FeatureType FeatureType { get; }
    public string CategoryName { get; }
    public EntityWithValueToUpdate[] Entities { get; }

    public DataToUpdate(FeatureType featureType, string categoryName, EntityWithValueToUpdate[] entities)
    {
        FeatureType = featureType;
        CategoryName = categoryName;
        Entities = entities;
    }
}
