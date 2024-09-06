using Quizanchos.Domain.Entities.Features;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.DbUpdater.Updater.FeatureUpdaters;

//TODO: Add this class to DI, make it as dependency of facory
internal class FeatureIntUpdater : IFeatureUpdater
{
    private readonly IFeatureIntRepository _featureIntRepository;
    private readonly int _value;
    private readonly QuizEntity _quizEntity;
    private readonly QuizCategory _quizCategory;

    public FeatureIntUpdater(IFeatureIntRepository featureIntRepository, int value, QuizEntity quizEntity, QuizCategory quizCategory)
    {
        _featureIntRepository = featureIntRepository;
        _value = value;
        _quizEntity = quizEntity;
        _quizCategory = quizCategory;
    }

    public async Task UpdateFeature()
    {
        FeatureInt featureInt = await _featureIntRepository.GetByCategoryAndEntity(_quizCategory.Id, _quizEntity.Id);

        if (featureInt is not null)
        {
            if(featureInt.Value != _value)
            {
                featureInt.Value = _value;
                await _featureIntRepository.Update(featureInt);
            }

            return;
        }

        featureInt = new FeatureInt
        {
            Id = Guid.NewGuid(),
            Value = _value,
            QuizEntity = _quizEntity,
            QuizCategory = _quizCategory
        };

        await _featureIntRepository.Create(featureInt);
    }
}
