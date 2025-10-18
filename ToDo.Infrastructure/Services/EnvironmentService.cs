namespace ToDo.Infrastructure.Services;

public static class EnvironmentService
{
	public static string SqlConnectionString => GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
	public static string EnvName => GetEnvironmentVariable("OtherEnvVariable");

	private static string GetEnvironmentVariable(string variable)
	{
		string? value = Environment.GetEnvironmentVariable(variable);

		if (string.IsNullOrWhiteSpace(value))
		{
			string errorMessage = $"Environment variable '{variable}' not found or is empty";
			throw new InvalidOperationException(errorMessage);
		}

		return value;
	}
}