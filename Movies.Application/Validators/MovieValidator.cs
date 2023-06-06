using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Application.Services;
using System.Runtime.InteropServices;

namespace Movies.Application.Validators
{
	public class MovieValidator : AbstractValidator<Movie>
	{

        private readonly IMovieRepository _movieRepository;

        public MovieValidator(IMovieRepository movieRepository)
        {
			_movieRepository = movieRepository;   

            RuleFor(m => m.Id)
                .NotEmpty();

            RuleFor(m => m.Title)
                .NotEmpty();

            RuleFor(m => m.Genres)
                .NotEmpty();

            RuleFor(m => m.YearOfRelease)
                .LessThanOrEqualTo(DateTime.UtcNow.Year);

            RuleFor(m => m.Slug)
                .MustAsync(ValidateSlug)
                .WithMessage("This movie already exists");
        }

        private async Task<bool> ValidateSlug(Movie movie, string slug, CancellationToken token = default)
        {
            var existingMovie = await _movieRepository.GetMovieBySlugAsync(slug);

            if(existingMovie is not null)
            {
                return existingMovie.Id == movie.Id;
            }
            else
            {
                return existingMovie is null;
            }
        }
    }
}
