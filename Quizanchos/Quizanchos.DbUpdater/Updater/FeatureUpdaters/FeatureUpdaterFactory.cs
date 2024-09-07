using Quizanchos.Common.FeatureTypes;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.DbUpdater.Updater.FeatureUpdaters;

internal class FeatureUpdaterFactory
{
    private readonly IFeatureIntRepository _featureIntRepository;
    private readonly IFeatureFloatRepository _featureFloatRepository;

    public FeatureUpdaterFactory(IFeatureIntRepository featureIntRepository, IFeatureFloatRepository featureFloatRepository)
    {
        _featureIntRepository = featureIntRepository;
        _featureFloatRepository = featureFloatRepository;
    }

    public IFeatureUpdater CreateFeatureUpdater(Type featureValueType, QuizCategory quizCategory)
    {
        if(featureValueType == typeof(FeatureValueInt))
        {
            return new FeatureIntUpdater(_featureIntRepository, quizCategory);
        }

        if(featureValueType == typeof(FeatureValueFloat))
        {
            return new FeatureFloatUpdater(_featureFloatRepository, quizCategory);
        }

        throw new NotImplementedException();
    }
}
