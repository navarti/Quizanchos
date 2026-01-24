using Quizanchos.Common.FeatureTypes;
using Quizanchos.DbUpdater.Updater;

namespace Quizanchos.DbUpdater.Entities;

internal class Currency
{
    public string Code { get; set; }
    public float Rate { get; set; }

    public Currency(string code, float rate)
    {
        Code = code;
        Rate = rate;
    }

    public EntityWithValueToUpdate ToUniversalEntityWithRate()
    {
        FeatureValueFloat featureValueFloat = new FeatureValueFloat(Rate);
        return new EntityWithValueToUpdate(Code, featureValueFloat);
    }
}
