using System.Reflection;
using System.Text.Json;

namespace Quizanchos.Plugin.CountryGuesser.Data;

public sealed class CountryRepository
{
    private readonly Lazy<IReadOnlyList<Country>> _countries;

    public CountryRepository()
    {
        _countries = new Lazy<IReadOnlyList<Country>>(LoadCountries);
    }

    public IReadOnlyList<Country> All => _countries.Value;

    private static IReadOnlyList<Country> LoadCountries()
    {
        var asmLocation = Path.GetDirectoryName(typeof(CountryRepository).Assembly.Location)
            ?? AppContext.BaseDirectory;
        var dataPath = Path.Combine(asmLocation, "Data", "countries.json");

        if (!File.Exists(dataPath))
        {
            // Fallback: countries.json may be alongside the dll instead of in a Data subfolder
            dataPath = Path.Combine(asmLocation, "countries.json");
        }
        if (!File.Exists(dataPath))
        {
            return Array.Empty<Country>();
        }

        var json = File.ReadAllText(dataPath);
        var list = JsonSerializer.Deserialize<List<Country>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        });
        return list ?? new List<Country>();
    }
}
