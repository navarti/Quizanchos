namespace Quizanchos.DbUpdater.Updater;

internal interface IDataUpdater
{
    public Task Update(DataToUpdate data);
}
