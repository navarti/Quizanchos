using Quizanchos.Common.Quiz.FeatureTypes;
using Quizanchos.Domain.Entities.Quiz;

namespace Quizanchos.DbUpdater.Updater.FeatureUpdaters;

internal interface IFeatureUpdater
{
    public Task UpdateFeature(FeatureValue value, QuizEntity quizEntity);
}
