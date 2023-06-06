using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Services
{
	public class MovieService : IMovieService
	{

		private readonly IMovieRepository _movierepository;
		private readonly IValidator<Movie> _movieValidator;
		private readonly IRatingRepository _ratingRepository;
		private readonly IValidator<GetAllMoviesOptions> _optionsValidator;

		public MovieService(IMovieRepository movieRepository, IValidator<Movie> movieValidator, IRatingRepository ratingRepository, IValidator<GetAllMoviesOptions> optionsValidator)
        {
            _movierepository= movieRepository;
			_movieValidator= movieValidator;
			_ratingRepository= ratingRepository;
			_optionsValidator = optionsValidator;
        }

        public async Task<bool> CreateMovieAsync(Movie movie, CancellationToken token = default)
		{
			await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken: token);

			return await _movierepository.CreateMovieAsync(movie, token);
		}

		public Task<bool> DeleteMovieByIdAsync(Guid id, CancellationToken token = default)
		{
			return _movierepository.DeleteMovieByIdAsync(id, token);
		}

		public async Task<IEnumerable<Movie>> GetAllMoviesAsync(GetAllMoviesOptions options, CancellationToken token = default)
		{
			await _optionsValidator.ValidateAndThrowAsync(options, cancellationToken: token);

			return await _movierepository.GetAllMoviesAsync(options, token);
		}

		public Task<int> GetCountAsync(string? title, int? yearOfRelease, CancellationToken token = default)
		{
			return _movierepository.GetCountAsync(title, yearOfRelease, token);
		}

		public async Task<Movie?> GetMovieByIdAsync(Guid id, Guid? userId = default, CancellationToken token = default)
		{
			return await _movierepository.GetMovieByIdAsync(id, userId, token);
		}

		public async Task<Movie?> GetMovieBySlugAsync(string slug, Guid? userId = default, CancellationToken token = default)
		{
			return await _movierepository.GetMovieBySlugAsync(slug, userId, token);	
		}

		public async Task<Movie?> UpdateMovieAsync(Movie movie, Guid? userId = default, CancellationToken token = default)
		{
			await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken: token);

			var movieExist = await _movierepository.ExistByIdAsync(movie.Id, token);
			if (!movieExist)
			{
				return null;
			}
			else
			{
				await _movierepository.UpdateMovieAsync(movie, token);
				if (!userId.HasValue)
				{
					var rating = await _ratingRepository.GetRatingAsync(movie.Id, token);
					movie.Rating = rating;
				}
				else
				{
					var ratings = await _ratingRepository.GetRatingAsync(movie.Id, userId, token);
					movie.UserRating = ratings.UserRating;
					movie.Rating = ratings.Rating;
				}
				return movie;
			}
		}
	}
}
