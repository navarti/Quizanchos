namespace Quizanchos.DbUpdater.Updater;

internal class DataToUpdate<T>
{
    public class EntityWithValueToUpdate
    {
        public string EntityName { get; }
        public T FeatureValue { get; }

        public EntityWithValueToUpdate(string entityName, T featureValue)
        {
            EntityName = entityName;
            FeatureValue = featureValue;
        }
    }
    
    public string CategoryName { get; }
    public EntityWithValueToUpdate[] Entities { get; }

    public DataToUpdate(string categoryName, EntityWithValueToUpdate[] entities)
    {
        CategoryName = categoryName;
        Entities = entities;
    }
}
