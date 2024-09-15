using Quizanchos.Common.FeatureTypes;
using Quizanchos.Domain.Entities;

namespace Quizanchos.DbUpdater.Updater.FeatureUpdaters;

internal interface IFeatureUpdater
{
    public Task UpdateFeature(FeatureValue value, QuizEntity quizEntity);
}
