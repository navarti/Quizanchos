namespace Quizanchos.DbUpdater.Updater;

public class EntityWithValueToUpdate<T>
{
    public string EntityName { get; }
    public T FeatureValue { get; }

    public EntityWithValueToUpdate(string entityName, T featureValue)
    {
        EntityName = entityName;
        FeatureValue = featureValue;
    }
}
