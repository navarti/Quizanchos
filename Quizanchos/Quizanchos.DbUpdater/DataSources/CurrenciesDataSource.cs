using Newtonsoft.Json.Linq;
using Quizanchos.DbUpdater.Entities;
using Quizanchos.DbUpdater.Utils;

namespace Quizanchos.DbUpdater.DataSources;

internal class CurrenciesDataSource
{
    private const string UrlFormat = "https://v6.exchangerate-api.com/v6/{0}/latest/USD";
    
    private readonly string _apiKey;
    private readonly List<Currency> _currencies = new();

    public CurrenciesDataSource(string apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task<List<Currency>> GetMovies()
    {
        _currencies.Clear();
        await AddCurrenciesToListAsync();
        return _currencies;
    }

    private async Task AddCurrenciesToListAsync()
    {
        string urlWithApiKey = AddApiKeyToUrl();

        // TODO: Make wrapper for HttpClient
        HttpClient client = new HttpClient();

        HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, urlWithApiKey);
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);
        string responseContent = await response.Content.ReadAsStringAsync();

        JToken parsedJson = JToken.Parse(responseContent);
        ProcessTokenAndAppend(parsedJson);
    }

    private void ProcessTokenAndAppend(JToken token)
    {
        JObject currencies = (JObject)token.GetToken("conversion_rates");
        foreach (JProperty currencyProperty in currencies.Properties())
        {
            Currency currency = new Currency(currencyProperty.Name, (float)currencyProperty.Value);
            _currencies.Add(currency); 
        }
    }

    private string AddApiKeyToUrl()
    {
        return string.Format(UrlFormat, _apiKey);
    }
}
