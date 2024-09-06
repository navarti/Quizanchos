using Quizanchos.DbUpdater.Updater.FeatureUpdaters;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.DbUpdater.Updater;

internal class DataUpdater : IDataUpdater
{
    private readonly IQuizCategoryRepository _quizCategoryRepository;
    private readonly IQuizEntityRepository _quizEntityRepository;
    private readonly IFeatureIntRepository _featureIntRepository;
    private readonly IFeatureFloatRepository _featureFloatRepository;

    public DataUpdater(
        IQuizCategoryRepository quizCategoryRepository, 
        IQuizEntityRepository quizEntityRepository, 
        IFeatureIntRepository featureIntRepository, 
        IFeatureFloatRepository featureFloatRepository)
    {
        _quizCategoryRepository = quizCategoryRepository;
        _quizEntityRepository = quizEntityRepository;
        _featureIntRepository = featureIntRepository;
        _featureFloatRepository = featureFloatRepository;
    }

    public async Task Update<T>(DataToUpdate<T> data)
    {
        QuizCategory quizCategory = await GetQuizCategory(data.CategoryName);

        foreach (EntityWithValueToUpdate<T> entity in data.Entities)
        {
            QuizEntity quizEntity = await GetQuizEntity(entity.EntityName);

            await UpdateFeatureValue(entity.FeatureValue, quizEntity, quizCategory);
        }
    }

    private async Task<QuizCategory> GetQuizCategory(string quizCategoryName)
    {
        QuizCategory quizCategory = await _quizCategoryRepository.GetByName(quizCategoryName);

        if (quizCategory is not null)
        {
            return quizCategory;
        }

        quizCategory = new QuizCategory();
        quizCategory.Id = Guid.NewGuid();
        quizCategory.Name = quizCategoryName;

        return _quizCategoryRepository.Create(quizCategory).Result;
    }

    private async Task<QuizEntity> GetQuizEntity(string entityName)
    {
        QuizEntity quizEntity = await _quizEntityRepository.GetByName(entityName);

        if(quizEntity is not null)
        {
            return quizEntity;
        }

        quizEntity = new QuizEntity();
        quizEntity.Id = Guid.NewGuid();
        quizEntity.Name = entityName;

        return _quizEntityRepository.Create(quizEntity).Result;
    }

    private async Task UpdateFeatureValue<T>(T value, QuizEntity quizEntity, QuizCategory quizCategory)
    {
        FeatureUpdaterFactory featureUpdaterFactory = new(_featureIntRepository, _featureFloatRepository);

        IFeatureUpdater featureUpdater = featureUpdaterFactory.CreateFeatureUpdater(value, quizEntity, quizCategory);
        await featureUpdater.UpdateFeature();
    }
}
