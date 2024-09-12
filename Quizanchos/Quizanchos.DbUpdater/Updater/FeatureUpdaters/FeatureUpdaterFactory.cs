using Quizanchos.Common.Enums;
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

    public IFeatureUpdater CreateFeatureUpdater(FeatureType featureType, QuizCategory quizCategory)
    {
        switch(featureType)
        {
            case FeatureType.Int:
                return new FeatureIntUpdater(_featureIntRepository, quizCategory);

            case FeatureType.Float:
                return new FeatureFloatUpdater(_featureFloatRepository, quizCategory);

            default:
                throw new NotImplementedException();
        }
    }
}
