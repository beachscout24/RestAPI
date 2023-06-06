﻿using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Application.Database
{
	public class DbInitializer
	{
		private readonly IDbConnectionFactory _connectionFactory;

        public DbInitializer(IDbConnectionFactory dbConnectionFactory)
        {
            _connectionFactory = dbConnectionFactory;
        }

        public async Task InitializeAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync("""
                create table if not exists movies (
                    id UUID primary key,
                    slug TEXT not null,
                    title TEXT not null,
                    yearOfRelease integer not null
                )
                """);

            await connection.ExecuteAsync(
                """
                create unique index concurrently if not exists movies_slug_idx
                on movies
                using btree(slug);
                """
                );

			await connection.ExecuteAsync("""
                create table if not exists genres (
                    movieId UUID references movies (Id),
                    name TEXT not null);
                """);

			await connection.ExecuteAsync("""
                create table if not exists ratings (
                    userid uuid,
                    movieid uuid references movies (id),
                    rating integer not null,
                    primary key (userid, movieid));
                """);
		}
    }
}
