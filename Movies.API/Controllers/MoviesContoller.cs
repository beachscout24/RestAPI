using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.API.Endpoints;
using Movies.API.Mapping;
using Movies.Application.Services;
using Movies.Contract.Requests;
using Movies.Contract.Responses;

namespace Movies.Api.Controllers
{
    [ApiController]
    [ApiVersion(1.0)]
    public class MoviesContoller : ControllerBase
    {

        private readonly IMovieService _movieService;
        private readonly IOutputCacheStore _outputCacheStore;

        public MoviesContoller(IMovieService movieService, IOutputCacheStore outputCacheStore)
        {
            _movieService = movieService;
            _outputCacheStore = outputCacheStore;
        }

        [Authorize(AuthConstants.TRUSTEDMEMBERPOLICYNAME)]
        [ServiceFilter(typeof(ApiKeyAuthFilter))]
        [HttpPost(ApiEndpoints.Movies.Create)]
        [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> CreateMovie([FromBody] CreateMovieRequest request, CancellationToken token)
        {
            var movie = request.MapToMovie();
            await _movieService.CreateMovieAsync(movie, token);
            await _outputCacheStore.EvictByTagAsync("movies", token);
            return CreatedAtAction(nameof(GetMovieByIdV1), new { idOrSlug = movie.Id }, movie);
        }

        [HttpGet(ApiEndpoints.Movies.Get)]
        [OutputCache(PolicyName = "MovieCache")]
       // [ResponseCache(Duration = 30, VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)]
		[ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetMovieByIdV1([FromRoute] string idOrSlug,
            CancellationToken token)
        {
            var userId = HttpContext.GetUserId();

            var movie = Guid.TryParse(idOrSlug, out var id)
                ? await _movieService.GetMovieByIdAsync(id, userId, token)
                : await _movieService.GetMovieBySlugAsync(idOrSlug, userId, token);

            if (movie is null)
            {
                return NotFound();
            }

            var response = movie.MapToResponse();
            return Ok(response);
        }

		
		[HttpGet(ApiEndpoints.Movies.GetAll)]
		[ProducesResponseType(typeof(MoviesResponse), StatusCodes.Status200OK)]
		[OutputCache(PolicyName = "MovieCache")]
		//[ResponseCache(Duration = 30, VaryByQueryKeys = new[] { "title", "year", "sortBy", "page", "pageSize"}, VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)]
		public async Task<IActionResult> GetAllMovies([FromQuery] GetAllMoviesRequest request, CancellationToken token)
        {
            var userId = HttpContext.GetUserId();
          
            var options = request.MapToOptions().WithUser(userId);

            var movies = await _movieService.GetAllMoviesAsync(options, token);

            var movieCount = await _movieService.GetCountAsync(options.Title, options.YearOfRelease, token);

            var response = movies.MapToResponse(request.Page, request.PageSize, movieCount);
            return Ok(response);
        }

        [Authorize(AuthConstants.TRUSTEDMEMBERPOLICYNAME)]
        [HttpPut(ApiEndpoints.Movies.Update)]
		[ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> UpdateMovie([FromRoute] Guid id, [FromBody] UpdateMovieRequest request, CancellationToken token)
        {
            var movie = request.MapToMovie(id);
            var userId = HttpContext.GetUserId();
            var updatedMovie = await _movieService.UpdateMovieAsync(movie, userId, token);

            if (updatedMovie is null)
            {
                return NotFound();
            }
            else
            {
                var response = movie.MapToResponse();
				await _outputCacheStore.EvictByTagAsync("movies", token);
				return Ok(response);
            }
        }

        [Authorize(AuthConstants.ADMINUSERPOLICYNAME)]
        [HttpDelete(ApiEndpoints.Movies.Delete)]
		[ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> DeleteMovie([FromRoute] Guid id, CancellationToken token)
        {
            var userId = HttpContext.GetUserId();
            var deleted = await _movieService.DeleteMovieByIdAsync(id, token);

            if (!deleted)
            {
                return NotFound();
            }
            else
            {
				await _outputCacheStore.EvictByTagAsync("movies", token);
				return Ok(deleted);
            }
        }
    }
}
