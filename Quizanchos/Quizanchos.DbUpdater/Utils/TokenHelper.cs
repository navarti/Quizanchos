using Newtonsoft.Json.Linq;

namespace Quizanchos.DbUpdater.Utils;

internal static class TokenHelper
{
    public static T GetOption<T>(this JToken jToken, string optionName)
    {
        JToken? optionToken = jToken[optionName];
        if (optionToken is null)
        {
            throw new ArgumentNullException($"Token does not have attribute {optionName}");
        }

        T? optionValue = optionToken.ToObject<T>();
        if(optionValue is null)
        {
            throw new ArgumentNullException($"Incorrect type of attribute {optionName}");
        }

        return optionValue;
    }

    public static bool HasOption(this JToken jToken, string optionName)
    {
        JToken? optionToken = jToken[optionName];
        return optionToken is null ? false : true;
    }

    public static JArray GetArray(this JToken jToken, string optionName)
    {
        JArray? tokens = jToken[optionName] as JArray;
        if (tokens is null)
        {
            throw new ArgumentNullException($"Token does not have attribute {optionName}");
        }

        return tokens;
    }
}
