using Newtonsoft.Json.Linq;
using Quizanchos.DbUpdater.Entities;

namespace Quizanchos.DbUpdater.DataSources;

internal class CountryDataSource
{
    public List<Exception> Exceptions { get; } = new List<Exception>();
    private List<Country> _countries = new List<Country>();

    public List<Country> GetCountriesSafe()
    {
        try
        {
            AddCountriesToListAsync().Wait();
        }
        catch (Exception ex)
        {
            Exceptions.Add(ex);
        }

        return _countries;
    }

    private async Task AddCountriesToListAsync()
    {
        // To fix
        HttpClient client = new HttpClient();

        string url = "https://restcountries.com/v3.1/all?fields=name,area,population";
        HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);

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
        try
        {
            string name = token["name"]["common"].ToString();
            float area = token["area"].ToObject<float>();
            int population = token["population"].ToObject<int>();

            Country country = new Country(name, area, population);
            _countries.Add(country);
        }
        catch(NullReferenceException ex)
        {
            Exceptions.Add(ex);
        }
    }
}
