using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories
{
	public class MovieRepository : IMovieRepository
	{
		private readonly IDbConnectionFactory _dbConnectionFactory;

        public MovieRepository(IDbConnectionFactory dbConnectionFactory)
        {
			_dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<bool> CreateMovieAsync(Movie movie, CancellationToken token)
		{
			using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
			using var transaction = connection.BeginTransaction();

			var result = await connection.ExecuteAsync(new CommandDefinition("""
				insert into movies (id, title, slug, yearofrelease) values (@Id, @Title, @Slug, @YearOfRelease)
				""", movie, cancellationToken: token));

			if(result > 0)
			{
				foreach (var genre in movie.Genres)
				{
					await connection.ExecuteAsync(new CommandDefinition("""
							insert into genres (movieId, name) values (@MovieId, @Name)
						""", new { MovieId = movie.Id, Name = genre}, cancellationToken: token));
				}
			}

			transaction.Commit();

			return result > 0;
		}

		public async Task<bool> DeleteMovieByIdAsync(Guid id, CancellationToken token = default)
		{
			using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
			using var transaction = connection.BeginTransaction();

			await connection.ExecuteAsync(new CommandDefinition("""
				delete from genres where movieid = @Id
				""", new { id }, cancellationToken: token));

			var result = await connection.ExecuteAsync(new CommandDefinition("""
				delete from movies where id = @Id
				""", new { id }, cancellationToken: token));

			transaction.Commit();
			return result > 0;
		}

		public async Task<bool> ExistByIdAsync(Guid id, CancellationToken token = default)
		{
			var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

			return await connection.ExecuteScalarAsync<bool>(new CommandDefinition("""
				select 1 from movies where id = @Id
				""", new { id }, cancellationToken: token));
		}

		public async Task<IEnumerable<Movie>> GetAllMoviesAsync(GetAllMoviesOptions options, CancellationToken token = default)
		{
			using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

			var orderClause = string.Empty;

			if(options.SortField is not null)
			{
				orderClause = $"""
					, m.{options.SortField} 
					order by m.{options.SortField} {(options.SortOrder == SortOrder.Ascending ? "asc" : "desc")}
					""";
			}

			var result = await connection.QueryAsync(new CommandDefinition($"""
            select m.*, 
                   string_agg(distinct g.name, ',') as genres , 
                   round(avg(r.rating), 1) as rating, 
                   myr.rating as userrating
            from movies m 
            left join genres g on m.id = g.movieid
            left join ratings r on m.id = r.movieid
            left join ratings myr on m.id = myr.movieid
                and myr.userid = @userId where (@title is null or m.title like ('%' || @title || '%')) and (@yearofrelease is null or m.yearofrelease = @yearofrelease) group by id, userrating {orderClause} limit @pageSize offset @pageOffset
            """, new { 
				userId = options.UserId,
				title = options.Title,
				yearofrelease = options.YearOfRelease,
				pageSize = options.PageSize,
				pageOffset = (options.Page - 1) * options.PageSize,
			}, cancellationToken: token));

			return result.Select(x => new Movie
			{
				Id = x.id,
				Title = x.title,
				YearOfRelease = x.yearofrelease,
				Rating = (float?)x.rating,
				UserRating = (int?)x.userrating,
				Genres = Enumerable.ToList(x.genres.Split(','))
			});
		}

		public async Task<int> GetCountAsync(string? title, int? yearOfRelease, CancellationToken token = default)
		{
			using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

			return await connection.QuerySingleAsync<int>(new CommandDefinition("""
				select count(id) from movies
				where (@title is null or title like ('%' || title || '%'))
				and (@yearofrelease is null or yearofrelease = @yearofrelease)
				""", new
			{
				title,
				yearOfRelease
			}, cancellationToken: token));
		}

		public async Task<Movie?> GetMovieByIdAsync(Guid id, Guid? userId = default, CancellationToken token = default)
		{
			using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

			var movie = await connection.QuerySingleOrDefaultAsync<Movie>(new CommandDefinition("""
					select m.* , round(avg(r.rating), 1) as rating, myr.rating as userrating
					from movies m
					left join ratings r on m.id = r.movieid
					left join ratings myr on m.id = myr.movieid 
					and myr.userid = @userId
					where id = @Id
					group by id, userrating
				""", new { id, userId }, cancellationToken: token));
			
			if(movie is null)
			{
				return null;
			}

			var genres = await connection.QueryAsync<string>(new CommandDefinition("""
				select name from genres where movieid = @Id
				""", new { id }, cancellationToken: token));

			foreach(var genre in genres)
			{
				movie.Genres.Add(genre);
			}

			return movie;
		}

		public async Task<Movie?> GetMovieBySlugAsync(string slug, Guid? userId = default, CancellationToken token = default)
		{
			using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

			var movie = await connection.QuerySingleOrDefaultAsync<Movie>(new CommandDefinition("""
					select m.* , round(avg(r.rating), 1) as rating, myr.rating as userrating
					from movies m
					left join ratings r on m.id = r.movieid
					left join ratings myr on m.id = myr.movieid 
					and myr.userid = @userId
					where slug = @slug
					group by id, userrating
				""", new { slug, userId }, cancellationToken: token));

			if (movie is null)
			{
				return null;
			}

			var genres = await connection.QueryAsync<string>(new CommandDefinition("""
				select name from genres where movieid = @Id
				""", new { id = movie.Id }, cancellationToken: token));

			foreach (var genre in genres)
			{
				movie.Genres.Add(genre);
			}

			return movie;
		}

		public async Task<bool> UpdateMovieAsync(Movie movie, CancellationToken token = default)
		{
			var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
			var transaction = connection.BeginTransaction();

			await connection.ExecuteAsync(new CommandDefinition("""
					delete from genres where movieId = @Id
				""", new {id =  movie.Id}, cancellationToken: token));

			foreach(var genre in movie.Genres)
			{
				await connection.ExecuteAsync(new CommandDefinition("""
						insert into genres (movieid, name) values (@MovieId, @Name)
					""", new { MovieId = movie.Id, Name = genre}, cancellationToken: token));
			}

			var result = await connection.ExecuteAsync(new CommandDefinition("""
				update movies set slug = @Slug, title = @Title, yearofrelease = @YearOfRelease where id = @Id
				""", movie, cancellationToken: token));

			transaction.Commit();

			return result > 0;
		}
	}
}
