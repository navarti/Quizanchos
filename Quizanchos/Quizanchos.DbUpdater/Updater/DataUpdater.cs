using Quizanchos.DbUpdater.Updater.FeatureUpdaters;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.DbUpdater.Updater;

internal class DataUpdater : IDataUpdater
{
    private readonly IQuizCategoryRepository _quizCategoryRepository;
    private readonly IQuizEntityRepository _quizEntityRepository;
    private readonly FeatureUpdaterFactory _featureUpdaterFactory;

    public DataUpdater(
        IQuizCategoryRepository quizCategoryRepository, 
        IQuizEntityRepository quizEntityRepository, 
        FeatureUpdaterFactory featureUpdaterFactory) 
    {
        _quizCategoryRepository = quizCategoryRepository;
        _quizEntityRepository = quizEntityRepository;
        _featureUpdaterFactory = featureUpdaterFactory;
    }

    public async Task Update(DataToUpdate data)
    {
        QuizCategory quizCategory = await GetQuizCategory(data.CategoryName);

        IFeatureUpdater featureUpdater = _featureUpdaterFactory.CreateFeatureUpdater(data.FeatureType, quizCategory);

        foreach (EntityWithValueToUpdate entity in data.Entities)
        {
            QuizEntity quizEntity = await GetQuizEntity(entity.EntityName);

            await featureUpdater.UpdateFeature(entity.FeatureValue, quizEntity);
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

        return await _quizCategoryRepository.Create(quizCategory);
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

        return await _quizEntityRepository.Create(quizEntity);
    }
}
