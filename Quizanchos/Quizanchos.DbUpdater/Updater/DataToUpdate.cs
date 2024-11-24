using Quizanchos.Common.Enums;

namespace Quizanchos.DbUpdater.Updater;

internal class DataToUpdate
{
    public FeatureType FeatureType { get; }
    public string CategoryName { get; }
    public string ImageUrl { get; }
    public EntityWithValueToUpdate[] Entities { get; }

    public DataToUpdate(FeatureType featureType, string categoryName, string imageUrl, EntityWithValueToUpdate[] entities)
    {
        FeatureType = featureType;
        CategoryName = categoryName;
        Entities = entities;
    }
}
