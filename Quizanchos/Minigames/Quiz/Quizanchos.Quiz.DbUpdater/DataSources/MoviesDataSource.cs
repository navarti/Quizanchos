using Newtonsoft.Json.Linq;
using Quizanchos.DbUpdater.Entities;
using Quizanchos.DbUpdater.Utils;

namespace Quizanchos.DbUpdater.DataSources;

internal class MoviesDataSource
{
    private const string Url = "https://api.themoviedb.org/3/discover/movie?";
    
    private readonly string _apiKey;
    private readonly int _maxAmountOfMovies;
    private readonly List<Movie> _movies = new List<Movie>();

    public MoviesDataSource(string apiKey, int maxAmountOfMovies)
    {
        _apiKey = apiKey;
        _maxAmountOfMovies = maxAmountOfMovies;
    }

    public async Task<List<Movie>> GetMovies()
    {
        _movies.Clear();
        await AddMoviesToListAsync();
        return _movies;
    }

    private async Task AddMoviesToListAsync()
    {
        string urlWithApiKey = AddApiKeyToUrl(Url);

        // TODO: Make wrapper for HttpClient
        HttpClient client = new HttpClient();

        bool stop = false;
        int pageCounter = 1;
        while (_movies.Count < _maxAmountOfMovies && !stop)
        {
            stop = await AddMoviesToListInPageAsync(client, urlWithApiKey, pageCounter++);
        }
    }

    private async Task<bool> AddMoviesToListInPageAsync(HttpClient client, string url, int page)
    {
        url = AddPageToUrl(url, page);
        
        HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);
        string responseContent = await response.Content.ReadAsStringAsync();

        JToken parsedJson = JToken.Parse(responseContent);
        if (parsedJson.HasOption("success"))
        {
            // success is always false
            return true;
        }

        ProcessTokenAndAppend(parsedJson);
        return false;
    }

    private void ProcessTokenAndAppend(JToken token)
    {
        JArray movies = token.GetArray("results");

        foreach (JToken movieToken in movies)
        {
            string title = movieToken.GetOption<string>("original_title");
            float rating = movieToken.GetOption<float>("vote_average");
            Movie movie = new Movie(title, rating);
            _movies.Add(movie);
        }
    }

    private string AddApiKeyToUrl(string url)
    {
        return $"{url}&api_key={_apiKey}";
    }
    
    private string AddPageToUrl(string url, int page)
    {
        return $"{url}&page={page}";
    }
}
