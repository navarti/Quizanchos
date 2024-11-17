using Quizanchos.Domain.Entities.Features;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.Common.FeatureTypes;

namespace Quizanchos.DbUpdater.Updater.FeatureUpdaters;

internal class FeatureIntUpdater : IFeatureUpdater
{
    private readonly IFeatureIntRepository _featureIntRepository;
    private readonly QuizCategory _quizCategory;

    public FeatureIntUpdater(IFeatureIntRepository featureIntRepository, QuizCategory quizCategory)
    {
        _featureIntRepository = featureIntRepository;
        _quizCategory = quizCategory;
    }

    public async Task UpdateFeature(FeatureValue value, QuizEntity quizEntity)
    {
        FeatureValueInt? featureValueInt = value as FeatureValueInt;
        if (featureValueInt is null)
        {
            throw new ArgumentException($"{nameof(value)} is not of type {nameof(FeatureValueInt)}");
        }

        FeatureInt? featureInt = await _featureIntRepository.FindByCategoryAndEntity(_quizCategory.Id, quizEntity.Id);
        if (featureInt is not null)
        {
            if(featureInt.Value != featureValueInt)
            {
                featureInt.Value = featureValueInt;
                await _featureIntRepository.Update(featureInt);
            }

            return;
        }

        featureInt = new FeatureInt
        {
            Value = featureValueInt,
            QuizEntity = quizEntity,
            QuizCategory = _quizCategory
        };

        await _featureIntRepository.Create(featureInt);
    }
}
