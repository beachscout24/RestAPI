using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
	public class RatingsController : ControllerBase
    {
        private readonly IRatingService _ratingService;
        public RatingsController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        [Authorize]
        [HttpPut(ApiEndpoints.Movies.Rate)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> RateMovie([FromRoute] Guid id, [FromBody] RateMovieRequest request, CancellationToken token)
        {
            var userId = HttpContext.GetUserId();

            var result = await _ratingService.RateMovieAsync(id, request.Rating, userId!.Value, token);

            return result ? Ok(result) : NotFound();
        }

        [Authorize]
        [HttpDelete(ApiEndpoints.Movies.DeleteRating)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> DeleteRatingAsync([FromRoute] Guid id, CancellationToken token)
        {
            var userId = HttpContext.GetUserId();

            var result = await _ratingService.DeleteRatingAsync(id, userId!.Value, token);

            return result ? Ok() : NotFound();
        }

        [Authorize]
        [HttpGet(ApiEndpoints.Rating.GetRatings)]
		[ProducesResponseType(typeof(MovieRatingResponse), StatusCodes.Status200OK)]
		public async Task<IActionResult> GetUserRatingsAsync(CancellationToken token = default)
        {
            var userId = HttpContext.GetUserId();
            var ratings = await _ratingService.GetRatingsForUserAsync(userId!.Value, token);

            var ratingsResponse = ratings.MapToResponse();

            return Ok(ratingsResponse);
        }
    }
}
