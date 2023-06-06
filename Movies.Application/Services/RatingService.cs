using FluentValidation;
using FluentValidation.Results;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Services
{
	public class RatingService : IRatingService
	{
		private readonly IRatingRepository _repository;
		private readonly IMovieRepository _movieRespository;	

        public RatingService(IRatingRepository repository, IMovieRepository movieRepository)
        {
            _repository = repository;
			_movieRespository = movieRepository;
        }

		public Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken token = default)
		{
			return _repository.DeleteRatingAsync(movieId, userId, token);
		}

		public async Task<IEnumerable<MovieRating>> GetRatingsForUserAsync(Guid userId, CancellationToken token = default)
		{
			return await _repository.GetRatingsForUserAsync(userId, token);
		}

		public async Task<bool> RateMovieAsync(Guid movieId, int rating, Guid userId, CancellationToken token = default)
		{
			if(rating is <= 0 or > 5)
			{
				throw new ValidationException(new[]
				{
					new ValidationFailure
					{
						PropertyName = "Rating",
						ErrorMessage = "Rating must be between 1 and 5"
					}
				});
			}

			var movieExist = await _movieRespository.ExistByIdAsync(movieId);

			if (!movieExist)
			{
				return false;
			}
			else
			{
				return await _repository.RateMovieAsync(movieId, rating, userId, token);
			}
		}
	}
}
