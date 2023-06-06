﻿using Movies.Application.Models;
using Movies.Contract.Requests;
using Movies.Contract.Responses;

namespace Movies.API.Mapping
{
    public static class ContractMapping
	{
		public static Movie MapToMovie(this CreateMovieRequest request)
		{
			return new Movie
			{
				Id = Guid.NewGuid(),
				Title = request.Title,
				YearOfRelease = request.YearOfRelease,
				Genres = request.Genres.ToList(),
			};
		}

		public static Movie MapToMovie(this UpdateMovieRequest request, Guid id)
		{
			return new Movie
			{
				Id = id,
				Title = request.Title,
				YearOfRelease = request.YearOfRelease,
				Genres = request.Genres.ToList()
			};
		}

		public static MovieResponse MapToResponse(this Movie movie)
		{
			return new MovieResponse
			{
				Id = movie.Id,
				Title = movie.Title,
				Slug = movie.Slug,
				Rating = movie.Rating,
				UserRating = movie.UserRating,
				YearOfRelease = movie.YearOfRelease,
				Genres = movie.Genres,
			};
		}

		public static MoviesResponse MapToResponse(this IEnumerable<Movie> movies, int Page, int PageSize, int PageCount)
		{
			return new MoviesResponse
			{
				Movies = movies.Select(MapToResponse),
				Page = Page,
				PageSize = PageSize,
				Total = PageCount
			};
		}

		public static IEnumerable<MovieRatingResponse> MapToResponse(this IEnumerable<MovieRating> movieRatings)
		{
			return movieRatings.Select(x => new MovieRatingResponse
			{
				Rating = x.Rating,
				Slug = x.Slug,
				MovieId	 = x.MovieId 
			});
		}

		public static GetAllMoviesOptions MapToOptions(this GetAllMoviesRequest request)
		{
			return new GetAllMoviesOptions
			{
				Title = request.Title,
				YearOfRelease = request.Year,
				SortField = request.SortBy?.Trim('+', '-'),
				SortOrder = request.SortBy is null ? SortOrder.Unsorted : 
					request.SortBy.StartsWith('-') ? SortOrder.Descending : SortOrder.Ascending,
				Page = request.Page,
				PageSize=request.PageSize,
			};
		}

		public static GetAllMoviesOptions WithUser( this GetAllMoviesOptions options, Guid? userId)
		{
			options.UserId = userId;
			return options;
		}
	}
}
