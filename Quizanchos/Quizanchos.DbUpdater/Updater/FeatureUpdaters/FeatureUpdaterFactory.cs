using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.DbUpdater.Updater.FeatureUpdaters;

//TODO: Add this class to DI, make it as dependency of dataupdater
internal class FeatureUpdaterFactory
{
    private readonly IFeatureIntRepository _featureIntRepository;
    private readonly IFeatureFloatRepository _featureFloatRepository;

    public FeatureUpdaterFactory(IFeatureIntRepository featureIntRepository, IFeatureFloatRepository featureFloatRepository)
    {
        _featureIntRepository = featureIntRepository;
        _featureFloatRepository = featureFloatRepository;
    }

    public IFeatureUpdater CreateFeatureUpdater<T>(T value, QuizEntity quizEntity, QuizCategory quizCategory)
    {
        switch (value)
        {
            case int valueInt:
                return new FeatureIntUpdater(_featureIntRepository, valueInt, quizEntity, quizCategory);

            case float valueFloat:
                return new FeatureFloatUpdater(_featureFloatRepository, valueFloat, quizEntity, quizCategory);

            default:
                throw new NotImplementedException();
        }
    }

}
