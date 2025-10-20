using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ToDo.Domain.Models;
using ToDo.Dtos;
using ToDo.Infrastructure.Data;
using Xunit;

namespace ToDo.IntegrationTests.Controllers;

public class ToDoItemControllerTests : IClassFixture<CustomWebApplicationFactory>
{
	private readonly HttpClient _client;
	private readonly CustomWebApplicationFactory _factory;

	public ToDoItemControllerTests(CustomWebApplicationFactory factory)
	{
		_factory = factory;
		_client = factory.CreateClient();
	}

	private ApplicationDbContext CreateDbContext()
	{
		IServiceScope scope = _factory.Services.CreateScope();
		return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	}

	[Fact]
	public async Task GetAll_ReturnsAllItems()
	{
		HttpResponseMessage response = await _client.GetAsync("/api/ToDoItem");
		response.EnsureSuccessStatusCode();

		List<ToDoItemResponse>? items = await response.Content.ReadFromJsonAsync<List<ToDoItemResponse>>();
		Assert.NotNull(items);
	}

	[Fact]
	public async Task Create_CreatesNewItem_WithValidData()
	{
		ToDoItemRequest request = new(
			DateTime.UtcNow.AddDays(1),
			"New Task",
			"New Description");

		HttpResponseMessage response = await _client.PostAsJsonAsync("/api/ToDoItem", request);
		Assert.Equal(HttpStatusCode.Created, response.StatusCode);

		Guid id = await response.Content.ReadFromJsonAsync<Guid>();
		Assert.NotEqual(Guid.Empty, id);

		await using ApplicationDbContext dbContext = CreateDbContext();
		ToDoItem? itemInDb = await dbContext.ToDoItems.FindAsync(id);
		Assert.NotNull(itemInDb);
		Assert.Equal(request.Title.Trim(), itemInDb.Title);
		Assert.Equal(request.Description.Trim(), itemInDb.Description);
		Assert.Equal(0, itemInDb.CompletionPercentage);
	}

	[Fact]
	public async Task Create_TrimsWhitespace_FromTitleAndDescription()
	{
		ToDoItemRequest request = new(
			DateTime.UtcNow.AddDays(1),
			"  Task with spaces  ",
			"  Description with spaces  ");

		HttpResponseMessage response = await _client.PostAsJsonAsync("/api/ToDoItem", request);
		Assert.Equal(HttpStatusCode.Created, response.StatusCode);

		Guid id = await response.Content.ReadFromJsonAsync<Guid>();
		await using ApplicationDbContext dbContext = CreateDbContext();

		ToDoItem? itemInDb = await dbContext.ToDoItems.FindAsync(id);
		Assert.NotNull(itemInDb);
		Assert.Equal("Task with spaces", itemInDb.Title);
		Assert.Equal("Description with spaces", itemInDb.Description);
	}

	[Fact]
	public async Task GetById_ReturnsItem_WhenExists()
	{
		await using ApplicationDbContext dbContext = CreateDbContext();
		ToDoItem item = new(DateTime.UtcNow.AddDays(1), "Existing", "Desc");
		dbContext.ToDoItems.Add(item);
		await dbContext.SaveChangesAsync();

		HttpResponseMessage response = await _client.GetAsync($"/api/ToDoItem/{item.Id}");
		response.EnsureSuccessStatusCode();

		ToDoItemResponse? fetched = await response.Content.ReadFromJsonAsync<ToDoItemResponse>();
		Assert.NotNull(fetched);
		Assert.Equal(item.Title, fetched.Title);
	}

	[Fact]
	public async Task GetById_ReturnsNotFound_WhenItemDoesNotExist()
	{
		HttpResponseMessage response = await _client.GetAsync($"/api/ToDoItem/{Guid.NewGuid()}");
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task Update_UpdatesItem_WhenExists()
	{
		await using ApplicationDbContext dbContext = CreateDbContext();
		ToDoItem item = new(DateTime.UtcNow.AddDays(1), "Old", "Desc");
		dbContext.ToDoItems.Add(item);
		await dbContext.SaveChangesAsync();

		ToDoItemRequest request = new(DateTime.UtcNow.AddDays(2), "Updated", "New Desc");

		HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/ToDoItem/{item.Id}", request);
		Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

		dbContext.Entry(item).State = EntityState.Detached;
		ToDoItem? updated = await dbContext.ToDoItems.AsNoTracking().FirstOrDefaultAsync(i => i.Id == item.Id);
		Assert.Equal("Updated", updated!.Title);
		Assert.Equal("New Desc", updated.Description);
	}

	[Fact]
	public async Task Update_ReturnsNotFound_WhenItemDoesNotExist()
	{
		ToDoItemRequest request = new(DateTime.UtcNow.AddDays(2), "Updated", "New Desc");
		HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/ToDoItem/{Guid.NewGuid()}", request);
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task MarkAsDone_SetsCompletionTo100_WhenItemExists()
	{
		await using ApplicationDbContext dbContext = CreateDbContext();
		ToDoItem item = new(DateTime.UtcNow.AddDays(1), "Done", "Desc");
		dbContext.ToDoItems.Add(item);
		await dbContext.SaveChangesAsync();

		HttpResponseMessage response = await _client.PutAsync($"/api/ToDoItem/{item.Id}/markAsDone", null);
		Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

		dbContext.Entry(item).State = EntityState.Detached;
		ToDoItem? updated = await dbContext.ToDoItems.AsNoTracking().FirstOrDefaultAsync(i => i.Id == item.Id);
		Assert.Equal(100, updated!.CompletionPercentage);
	}

	[Fact]
	public async Task MarkAsDone_ReturnsNotFound_WhenItemDoesNotExist()
	{
		HttpResponseMessage response = await _client.PutAsync($"/api/ToDoItem/{Guid.NewGuid()}/markAsDone", null);
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task Delete_DeletesItem_WhenItemExists()
	{
		await using ApplicationDbContext dbContext = CreateDbContext();
		ToDoItem item = new(DateTime.UtcNow.AddDays(1), "Task to Delete", "Description");
		dbContext.ToDoItems.Add(item);
		await dbContext.SaveChangesAsync();

		HttpResponseMessage response = await _client.DeleteAsync($"/api/ToDoItem/{item.Id}");
		Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

		ToDoItem? deleted = await dbContext.ToDoItems.AsNoTracking()
			.FirstOrDefaultAsync(i => i.Id == item.Id);

		Assert.Null(deleted);
	}

	[Fact]
	public async Task Delete_ReturnsNotFound_WhenItemDoesNotExist()
	{
		HttpResponseMessage response = await _client.DeleteAsync($"/api/ToDoItem/{Guid.NewGuid()}");
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task GetIncoming_ReturnsItemsWithinPeriod()
	{
		DateTime now = DateTime.UtcNow;
		await using ApplicationDbContext dbContext = CreateDbContext();
		dbContext.ToDoItems.AddRange(
			new ToDoItem(now.AddDays(1), "Tomorrow", "Soon"),
			new ToDoItem(now.AddDays(10), "Next Week", "Later"));
		await dbContext.SaveChangesAsync();

		HttpResponseMessage response = await _client.GetAsync("/api/ToDoItem/incoming?incomingPeriod=Week");
		response.EnsureSuccessStatusCode();

		List<ToDoItemResponse>? items = await response.Content.ReadFromJsonAsync<List<ToDoItemResponse>>();
		Assert.NotNull(items);
		Assert.Contains(items, i => i.Title == "Tomorrow");
	}
}