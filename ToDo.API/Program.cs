using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ToDo.Application.Interfaces.Repositories;
using ToDo.Application.Interfaces.Services;
using ToDo.Application.Services;
using ToDo.Infrastructure.Data;
using ToDo.Infrastructure.Health;
using ToDo.Infrastructure.Repositories;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IToDoItemRepository, ToDoItemRepository>();

builder.Services.AddHealthChecks()
	.AddCheck<DatabaseHealthCheck>("custom-sql", HealthStatus.Unhealthy);

builder.Services.AddScoped<IToDoItemService, ToDoItemService>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

using (IServiceScope scope = app.Services.CreateScope())
{
	ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	if (db.Database.IsRelational())
		db.Database.Migrate();
}

app.MapControllers();

app.MapHealthChecks("health");

app.Run();

public partial class Program
{
}