using Movies.Application.Models;

namespace Movies.Application.Repositories
{
	public interface IMovieRepository
	{
		Task<bool> CreateMovieAsync(Movie movie, CancellationToken token = default);

		Task<Movie?> GetMovieByIdAsync(Guid id, Guid? userId = default, CancellationToken token = default);

		Task<Movie?> GetMovieBySlugAsync(string slug, Guid? userId = default, CancellationToken token = default);

		Task<IEnumerable<Movie>> GetAllMoviesAsync(GetAllMoviesOptions options, CancellationToken token = default);

		Task<bool> UpdateMovieAsync(Movie movie, CancellationToken token = default);

		Task<bool> DeleteMovieByIdAsync(Guid id, CancellationToken token = default);

		Task<bool> ExistByIdAsync(Guid id, CancellationToken token = default);

		Task<int> GetCountAsync(string? title, int? yearOfRelease, CancellationToken token = default);
	}
}
