using Newtonsoft.Json.Linq;
using Quizanchos.DbUpdater.Entities;

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
        string name = GetName(token);
        float area = GetArea(token);
        int population = GetPopulation(token);
            
        Country country = new Country(name, area, population);
        _countries.Add(country);
    }

    private string GetName(JToken token)
    {
        const string TokenName = "name";
        const string TokenCommon = "common";

        string? name = token[TokenName]?[TokenCommon]?.ToString();
        if(name is null)
        {
            ThrowAttribute($"{TokenName}.{TokenCommon}");
        }
        return name;
    }

    private float GetArea(JToken token)
    {
        const string TokenArea = "area";

        float? area = token[TokenArea]?.ToObject<float>();
        if (area is null)
        {
            ThrowAttribute(TokenArea);
        }
        return area.Value;
    }

    private int GetPopulation(JToken token)
    {
        const string TokenPopulation = "population";

        int? population = token[TokenPopulation]?.ToObject<int>();
        if (population is null)
        {
            ThrowAttribute(TokenPopulation);
        }
        return population.Value;
    }

    private void ThrowAttribute(string attributeName)
    {
        throw new ArgumentNullException($"Token does not have attribute {attributeName}");
    }
}
