using Quizanchos.Common.FeatureTypes;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Entities.Features;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.DbUpdater.Updater.FeatureUpdaters;

internal class FeatureFloatUpdater : IFeatureUpdater
{
    private readonly IFeatureFloatRepository _featureFloatRepository;
    private readonly QuizCategory _quizCategory;

    public FeatureFloatUpdater(IFeatureFloatRepository featureFloatRepository, QuizCategory quizCategory)
    {
        _featureFloatRepository = featureFloatRepository;
        _quizCategory = quizCategory;
    }

    public async Task UpdateFeature(FeatureValue value, QuizEntity quizEntity)
    {
        FeatureValueFloat? featureValueFloat = value as FeatureValueFloat;
        if (featureValueFloat is null)
        {
            throw new ArgumentException($"{nameof(value)} is not of type {nameof(FeatureValueFloat)}");
        }

        FeatureFloat? featureFloat = await _featureFloatRepository.FindByCategoryAndEntity(_quizCategory.Id, quizEntity.Id);
        if (featureFloat is not null)
        {
            if (featureFloat.Value != featureValueFloat)
            {
                featureFloat.Value = featureValueFloat;
                await _featureFloatRepository.Update(featureFloat);
            }

            return;
        }

        featureFloat = new FeatureFloat
        {
            Value = featureValueFloat,
            QuizEntity = quizEntity,
            QuizCategory = _quizCategory
        };

        await _featureFloatRepository.Create(featureFloat);
    }
}
