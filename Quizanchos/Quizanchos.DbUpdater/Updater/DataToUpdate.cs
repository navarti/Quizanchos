namespace Quizanchos.DbUpdater.Updater;

internal class DataToUpdate<T>
{
    public string CategoryName { get; }
    public EntityWithValueToUpdate<T>[] Entities { get; }

    public DataToUpdate(string categoryName, EntityWithValueToUpdate<T>[] entities)
    {
        CategoryName = categoryName;
        Entities = entities;
    }
}
