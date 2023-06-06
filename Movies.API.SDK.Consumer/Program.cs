using Microsoft.Extensions.DependencyInjection;
using Movies.API.SDK;
using Movies.API.SDK.Consumer;
using Movies.Contract.Requests;
using Refit;
using System.Text.Json;

//var movieApi = RestService.For<IMoviesAPI>("https://localhost:5001");

/*var services = new ServiceCollection();
services
	.AddHttpClient()
	.AddSingleton<AuthTokenProvider>()
	.AddRefitClient<IMoviesAPI>(s => new RefitSettings
	{
		AuthorizationHeaderValueGetter = async () => await s.GetRequiredService<AuthTokenProvider>().GetTokenAsync()
	})
	.ConfigureHttpClient(x =>
	{
		x.BaseAddress = new Uri("https://localhost:5001");
	});

var provider = services.BuildServiceProvider();
var movieApi = provider.GetRequiredService<IMoviesAPI>();
var movie = await movieApi.GetMovieAsync("jumanji-2021");

var newMovie = await movieApi.CreateMovieAsync(new CreateMovieRequest
{
	Title = "Spiderman 3",
	YearOfRelease = 2006,
	Genres = new[] {
		"Action"
	}
});

await movieApi.UpdateMovieAsync(newMovie.Id, new UpdateMovieRequest
{
	Title = "Spiderman 3",
	YearOfRelease = 2006,
	Genres = new[] {
		"Action",
		"Adventure"
	}
});

await movieApi.DeleteMovieAsync(newMovie.Id);

var request = new GetAllMoviesRequest
{
	Title = null,
	Year = null,
	SortBy = null,
	Page = 1,
	PageSize = 3,
};
var movies = await movieApi.GetAllMoviesAsync(request);

//Console.WriteLine(movie);
//Console.WriteLine(JsonSerializer.Serialize(movie));

foreach(var movieResponse in movies.Movies)
{
	Console.WriteLine(movieResponse);
	Console.WriteLine(JsonSerializer.Serialize(movieResponse));
}
*/

var services = new ServiceCollection();

services
	.AddHttpClient()
	.AddSingleton<AuthTokenProvider>()
	.AddRefitClient<IMoviesApi>(s => new RefitSettings
	{
		AuthorizationHeaderValueGetter = async () => await s.GetRequiredService<AuthTokenProvider>().GetTokenAsync()
	})
	.ConfigureHttpClient(x =>
		x.BaseAddress = new Uri("https://localhost:5001"));

var provider = services.BuildServiceProvider();

var moviesApi = provider.GetRequiredService<IMoviesApi>();

var movie = await moviesApi.GetMovieAsync("nick-the-greek-2023");

var newMovie = await moviesApi.CreateMovieAsync(new CreateMovieRequest
{
	Title = "Spiderman 2",
	YearOfRelease = 2002,
	Genres = new[] { "Action" }
});

await moviesApi.UpdateMovieAsync(newMovie.Id, new UpdateMovieRequest()
{
	Title = "Spiderman 2",
	YearOfRelease = 2002,
	Genres = new[] { "Action", "Adventure" }
});

await moviesApi.DeleteMovieAsync(newMovie.Id);

var request = new GetAllMoviesRequest
{
	Title = null,
	Year = null,
	SortBy = null,
	Page = 1,
	PageSize = 3
};

var movies = await moviesApi.GetMoviesAsync(request);

foreach (var movieResponse in movies.Movies)
{
	Console.WriteLine(JsonSerializer.Serialize(movieResponse));
}
