using Newtonsoft.Json.Linq;
using Quizanchos.DbUpdater.Entities;
using Quizanchos.DbUpdater.Utils;

namespace Quizanchos.DbUpdater.DataSources;

internal class CountryDataSource
{
    private const string Url = "https://restcountries.com/v3.1/all?fields=name,area,population";
    private List<Country> _countries = new List<Country>();

    public async Task<List<Country>> GetCountries()
    {
        _countries.Clear();
        await AddCountriesToListAsync();
        return _countries;
    }

    private async Task AddCountriesToListAsync()
    {
        // TODO: Make wrapper for HttpClient
        HttpClient client = new HttpClient();

        HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Url);

        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);
        string responseContent = await response.Content.ReadAsStringAsync();

        JArray parsedJson = JArray.Parse(responseContent);
        foreach (JToken token in parsedJson)
        {
            ProcessTokenAndAppend(token);
        }
    }

    private void ProcessTokenAndAppend(JToken token)
    {
        string name = token.GetOption<string>("area");
        float area = token.GetOption<int>("area");
        int population = token.GetOption<int>("population");
            
        Country country = new Country(name, area, population);
        _countries.Add(country);
    }
}
