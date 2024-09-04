using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Entities.Features;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.DbUpdater.Updater;

internal class DbUpdater
{
    private readonly IQuizCategoryRepository _quizCategoryRepository;
    private readonly IQuizEntityRepository _quizEntityRepository;
    private readonly IFeatureIntRepository _featureIntRepository;
    private readonly IFeatureFloatRepository _featureFloatRepository;

    public DbUpdater(
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

    public void UpdateEntities<T>(DataToUpdate<T> data)
    {
        QuizCategory quizCategory = GetQuizCategory(data.CategoryName);

        foreach (DataToUpdate<T>.EntityWithValueToUpdate entity in data.Entities)
        {
            QuizEntity quizEntity = GetQuizEntity(entity.EntityName);

            UpdateFeatureValue(entity.FeatureValue, quizEntity, quizCategory);
        }
    }

    private QuizCategory GetQuizCategory(string quizCategoryName)
    {
        QuizCategory quizCategory = _quizCategoryRepository.GetByName(quizCategoryName).Result;

        if (quizCategory is not null)
        {
            return quizCategory;
        }

        quizCategory = new QuizCategory();
        quizCategory.Id = Guid.NewGuid();
        quizCategory.Name = quizCategoryName;

        return _quizCategoryRepository.Create(quizCategory).Result;
    }

    private QuizEntity GetQuizEntity(string entityName)
    {
        QuizEntity quizEntity = _quizEntityRepository.GetByName(entityName).Result;

        if(quizEntity is not null)
        {
            return quizEntity;
        }

        quizEntity = new QuizEntity();
        quizEntity.Id = Guid.NewGuid();
        quizEntity.Name = entityName;

        return _quizEntityRepository.Create(quizEntity).Result;
    }

    private void UpdateFeatureValue<T>(T value, QuizEntity quizEntity, QuizCategory quizCategory)
    {
        switch(value)
        {
            case int valueInt:
                UpdateFeatureInt(valueInt, quizEntity, quizCategory);
                break;
            
            case float valueFloat:
                UpdateFeatureFloat(valueFloat, quizEntity, quizCategory);
                break;
            
            default:
                throw new NotImplementedException();
        }
    }

    private void UpdateFeatureInt(int value, QuizEntity quizEntity, QuizCategory quizCategory)
    {
        FeatureInt featureInt = _featureIntRepository.GetByCategoryAndEntity(quizCategory.Id, quizEntity.Id).Result;
        if(featureInt is not null)
        {
            featureInt.Value = value;
            _featureIntRepository.Update(featureInt).Wait();
            return;
        }

        featureInt = new FeatureInt();
        featureInt.Id = Guid.NewGuid();
        featureInt.QuizCategory = quizCategory;
        featureInt.QuizEntity = quizEntity;
        featureInt.Value = value;
        _featureIntRepository.Create(featureInt).Wait();
    }

    private void UpdateFeatureFloat(float value, QuizEntity quizEntity, QuizCategory quizCategory)
    {
        FeatureFloat featureFloat = _featureFloatRepository.GetByCategoryAndEntity(quizCategory.Id, quizEntity.Id).Result;
        if (featureFloat is not null)
        {
            featureFloat.Value = value;
            _featureFloatRepository.Update(featureFloat).Wait();
            return;
        }

        featureFloat = new FeatureFloat();
        featureFloat.Id = Guid.NewGuid();
        featureFloat.QuizCategory = quizCategory;
        featureFloat.QuizEntity = quizEntity;
        featureFloat.Value = value;
        _featureFloatRepository.Create(featureFloat).Wait();
    }
}
