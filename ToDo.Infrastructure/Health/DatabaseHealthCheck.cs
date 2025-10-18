using Npgsql;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ToDo.Infrastructure.Services;

namespace ToDo.Infrastructure.Health;

public sealed class DatabaseHealthCheck : IHealthCheck
{
	public async Task<HealthCheckResult> CheckHealthAsync(
		HealthCheckContext context,
		CancellationToken cancellationToken = new())
	{
		try
		{
			string connectionString = EnvironmentService.SqlConnectionString;

			await using NpgsqlConnection connection = new(connectionString);
			await connection.OpenAsync(cancellationToken);

			await using NpgsqlCommand command = new("SELECT 1", connection);
			await command.ExecuteScalarAsync(cancellationToken);

			return HealthCheckResult.Healthy();
		}
		catch (Exception e)
		{
			return HealthCheckResult.Unhealthy(exception: e);
		}
	}
}