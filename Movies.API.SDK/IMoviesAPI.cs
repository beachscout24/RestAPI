using Movies.Contract.Requests;
using Movies.Contract.Responses;
using Refit;

namespace Movies.API.SDK
{
	/*[Headers("Authorization: Bearer")]
	public interface IMoviesAPI
	{
		[Get(ApiEndpoints.Movies.Get)]
		Task<MovieResponse> GetMovieAsync(string idOrSlug);

		[Get(ApiEndpoints.Movies.GetAll)]
		Task<MoviesResponse> GetAllMoviesAsync(GetAllMoviesRequest getAllMoviesRequest);

		[Post(ApiEndpoints.Movies.Create)]
		public Task<MovieResponse> CreateMovieAsync(CreateMovieRequest createMovieRequest);

		[Put(ApiEndpoints.Movies.Update)]
		public Task<MovieResponse> UpdateMovieAsync(Guid id, UpdateMovieRequest updateMovieRequest);

		[Delete(ApiEndpoints.Movies.Delete)]
		public Task<MovieResponse> DeleteMovieAsync(Guid id);

		[Put(ApiEndpoints.Movies.Rate)]
		public Task RateMovieAsync(Guid id, RateMovieRequest rateMovieRequest);

		[Delete(ApiEndpoints.Movies.DeleteRating)]
		public Task DeleteRatingAsync(Guid id);

		[Get(ApiEndpoints.Rating.GetRatings)]
		public Task<IEnumerable<MovieRatingResponse>> GetUSerRatingsAsync();

	}*/

	[Headers("Authorization: Bearer")]
	public interface IMoviesApi
	{
		[Get(ApiEndpoints.Movies.Get)]
		Task<MovieResponse> GetMovieAsync(string idOrSlug);

		[Get(ApiEndpoints.Movies.GetAll)]
		Task<MoviesResponse> GetMoviesAsync(GetAllMoviesRequest request);

		[Post(ApiEndpoints.Movies.Create)]
		Task<MovieResponse> CreateMovieAsync(CreateMovieRequest request);

		[Put(ApiEndpoints.Movies.Update)]
		Task<MovieResponse> UpdateMovieAsync(Guid id, UpdateMovieRequest request);

		[Delete(ApiEndpoints.Movies.Delete)]
		Task DeleteMovieAsync(Guid id);

		[Put(ApiEndpoints.Movies.Rate)]
		Task RateMovieAsync(Guid id, RateMovieRequest request);

		[Delete(ApiEndpoints.Movies.DeleteRating)]
		Task DeleteRatingAsync(Guid id);

		[Get(ApiEndpoints.Rating.GetRatings)]
		Task<IEnumerable<MovieRatingResponse>> GetUserRatingsAsync();
	}
}
