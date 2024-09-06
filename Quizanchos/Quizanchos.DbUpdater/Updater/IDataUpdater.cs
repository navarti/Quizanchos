namespace Quizanchos.DbUpdater.Updater;

internal interface IDataUpdater
{
    public Task Update<T>(DataToUpdate<T> data);
}
