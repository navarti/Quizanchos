using Newtonsoft.Json.Linq;

namespace Quizanchos.DbUpdater.Utils;

internal static class TokenHelper
{
    public static JToken GetToken(this JToken jToken, string tokenName)
    {
        JToken optionToken = jToken[tokenName] 
            ?? throw new ArgumentNullException($"Token does not have attribute {tokenName}");
        return optionToken;
    }

    public static T GetOption<T>(this JToken jToken, string optionName)
    {
        JToken optionToken = jToken[optionName] 
            ?? throw new ArgumentNullException($"Token does not have attribute {optionName}");

        T optionValue = optionToken.ToObject<T>()
            ?? throw new ArgumentNullException($"Incorrect type of attribute {optionName}");

        return optionValue;
    }

    public static bool HasOption(this JToken jToken, string optionName)
    {
        return jToken[optionName] is null ? false : true;
    }

    public static JArray GetArray(this JToken jToken, string optionName)
    {
        JArray tokens = jToken[optionName] as JArray
            ?? throw new ArgumentNullException($"Token does not have attribute {optionName}");
        return tokens;
    }
}
