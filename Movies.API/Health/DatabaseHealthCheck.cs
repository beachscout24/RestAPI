using Microsoft.Extensions.Diagnostics.HealthChecks;
using Movies.Application.Database;

namespace Movies.Api.Health
{
	public class DatabaseHealthCheck : IHealthCheck
	{

		private readonly IDbConnectionFactory _connectionFactory;
		private readonly ILogger<DatabaseHealthCheck> _logger;
		public const string Name = "Database";

        public DatabaseHealthCheck(IDbConnectionFactory connectionFactory, ILogger<DatabaseHealthCheck> logger)
        {
            _connectionFactory = connectionFactory;
			_logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
			HealthCheckContext context, 
			CancellationToken cancellationToken = new())
		{
			try
			{
				_ = await _connectionFactory.CreateConnectionAsync(cancellationToken);
				return HealthCheckResult.Healthy();
			}
			catch(Exception ex)
			{
				const string errorMessage = "Database is unhealthy";
				_logger.LogError(errorMessage, ex);
				return HealthCheckResult.Unhealthy(errorMessage, ex);
			}
		}
	}
}
