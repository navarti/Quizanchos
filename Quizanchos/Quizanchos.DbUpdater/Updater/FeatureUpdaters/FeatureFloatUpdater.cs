using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Entities.Features;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.DbUpdater.Updater.FeatureUpdaters;

//TODO: Add this class to DI, make it as dependency of facory
internal class FeatureFloatUpdater : IFeatureUpdater
{
    private readonly IFeatureFloatRepository _featureFloatRepository;
    private readonly float _value;
    private readonly QuizEntity _quizEntity;
    private readonly QuizCategory _quizCategory;

    public FeatureFloatUpdater(IFeatureFloatRepository featureFloatRepository, float value, QuizEntity quizEntity, QuizCategory quizCategory)
    {
        _featureFloatRepository = featureFloatRepository;
        _value = value;
        _quizEntity = quizEntity;
        _quizCategory = quizCategory;
    }

    public async Task UpdateFeature()
    {
        FeatureFloat featureFloat = await _featureFloatRepository.GetByCategoryAndEntity(_quizCategory.Id, _quizEntity.Id);

        if (featureFloat is not null)
        {
            if (featureFloat.Value != _value)
            {
                featureFloat.Value = _value;
                await _featureFloatRepository.Update(featureFloat);
            }

            return;
        }

        featureFloat = new FeatureFloat
        {
            Id = Guid.NewGuid(),
            Value = _value,
            QuizEntity = _quizEntity,
            QuizCategory = _quizCategory
        };

        await _featureFloatRepository.Create(featureFloat);
    }
}
