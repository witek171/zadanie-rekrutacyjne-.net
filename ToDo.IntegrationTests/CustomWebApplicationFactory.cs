using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ToDo.Infrastructure.Data;

namespace ToDo.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureServices(services =>
		{
			ServiceDescriptor? descriptor = services.SingleOrDefault(
				d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

			if (descriptor != null)
				services.Remove(descriptor);

			services.AddDbContext<ApplicationDbContext>(options => { options.UseInMemoryDatabase("TestDatabase"); });

			ServiceProvider serviceProvider = services.BuildServiceProvider();
			using IServiceScope scope = serviceProvider.CreateScope();
			ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

			db.Database.EnsureCreated();
		});
	}
}