using Quizanchos.Common.FeatureTypes;
using Quizanchos.Quiz.Entities;

namespace Quizanchos.DbUpdater.Updater.FeatureUpdaters;

internal interface IFeatureUpdater
{
    public Task UpdateFeature(FeatureValue value, QuizEntity quizEntity);
}
