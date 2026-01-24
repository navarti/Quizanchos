using Quizanchos.Common.FeatureTypes;

namespace Quizanchos.DbUpdater.Updater;

public class EntityWithValueToUpdate
{
    public string EntityName { get; }
    public FeatureValue FeatureValue { get; }

    public EntityWithValueToUpdate(string entityName, FeatureValue featureValue)
    {
        EntityName = entityName;
        FeatureValue = featureValue;
    }
}
