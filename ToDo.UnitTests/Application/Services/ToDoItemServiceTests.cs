using Moq;
using ToDo.Application.Interfaces.Repositories;
using ToDo.Application.Services;
using ToDo.Domain.Models;
using ToDo.Domain.Models.Enums;
using Xunit;

namespace ToDo.UnitTests.Application.Services;

public class ToDoItemServiceTests
{
	private readonly Mock<IToDoItemRepository> _toDoItemRepository;
	private readonly ToDoItemService _toDoItemService;

	public ToDoItemServiceTests()
	{
		_toDoItemRepository = new Mock<IToDoItemRepository>();
		_toDoItemService = new ToDoItemService(_toDoItemRepository.Object);
	}

	private static ToDoItem CreateToDoItem(
		Guid id,
		DateTime expirationDate,
		string title,
		string description,
		int completionPercentage)
	{
		ToDoItem toDoItem = new(expirationDate, title, description);

		typeof(ToDoItem).GetProperty(nameof(ToDoItem.Id))!
			.SetValue(toDoItem, id);

		typeof(ToDoItem).GetProperty(nameof(ToDoItem.CompletionPercentage))!
			.SetValue(toDoItem, completionPercentage);

		return toDoItem;
	}

	[Fact]
	public async Task GetByIdAsync_WhenItemExists_ShouldReturnItem()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		ToDoItem item = CreateToDoItem(id, new DateTime(2025, 10, 25), "Test Title", "Test Description", 0);

		_toDoItemRepository
			.Setup(r => r.GetByIdAsync(id))
			.ReturnsAsync(item);

		// Act
		ToDoItem? result = await _toDoItemService.GetByIdAsync(id);

		// Assert
		Assert.NotNull(result);
		Assert.Same(item, result);
	}

	[Fact]
	public async Task GetByIdAsync_WhenItemDoesNotExist_ShouldReturnNull()
	{
		// Arrange
		Guid id = Guid.NewGuid();

		_toDoItemRepository
			.Setup(r => r.GetByIdAsync(id))
			.ReturnsAsync((ToDoItem?)null);

		// Act
		ToDoItem? result = await _toDoItemService.GetByIdAsync(id);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public async Task GetAllAsync_WhenItemsExist_ShouldReturnAllItemsFromRepository()
	{
		// Arrange
		List<ToDoItem> expectedItems = new()
		{
			CreateToDoItem(Guid.NewGuid(), new DateTime(2025, 10, 24), "Task 1", "Description 1", 0),
			CreateToDoItem(Guid.NewGuid(), new DateTime(2025, 10, 25), "Task 2", "Description 2", 50),
			CreateToDoItem(Guid.NewGuid(), new DateTime(2025, 10, 26), "Task 3", "Description 3", 100)
		};

		_toDoItemRepository
			.Setup(r => r.GetAllAsync())
			.ReturnsAsync(expectedItems);

		// Act
		IEnumerable<ToDoItem> result = await _toDoItemService.GetAllAsync();

		// Assert
		Assert.NotNull(result);
		Assert.Equal(expectedItems, result);
		_toDoItemRepository.Verify(r => r.GetAllAsync(), Times.Once);
	}

	[Fact]
	public async Task GetAllAsync_WhenNoItemsExist_ShouldReturnEmptyCollection()
	{
		// Arrange
		List<ToDoItem> emptyList = new();

		_toDoItemRepository
			.Setup(r => r.GetAllAsync())
			.ReturnsAsync(emptyList);

		// Act
		IEnumerable<ToDoItem> result = await _toDoItemService.GetAllAsync();

		// Assert
		Assert.NotNull(result);
		Assert.Empty(result);
		_toDoItemRepository.Verify(r => r.GetAllAsync(), Times.Once);
	}

	[Fact]
	public async Task AddAsync_WhenCalled_ShouldNormalizeTitleAndDescriptionAndReturnId()
	{
		// Arrange
		ToDoItem itemToAdd = new(new DateTime(2025, 10, 25), " test ", " Some description ");
		Guid id = Guid.NewGuid();

		_toDoItemRepository.Setup(r => r.AddAsync(It.IsAny<ToDoItem>())).ReturnsAsync(id);

		// Act
		Guid newItemId = await _toDoItemService.AddAsync(itemToAdd);

		// Assert
		Assert.Equal(id, newItemId);
		_toDoItemRepository.Verify(r => r.AddAsync(
			It.Is<ToDoItem>(x => x.Title == "test" && x.Description == "Some description")), Times.Once);
	}

	[Fact]
	public async Task UpdateAsync_WhenCalled_ShouldNormalizeTitleAndDescriptionAndCallRepository()
	{
		// Arrange
		ToDoItem itemToUpdate = new(new DateTime(2025, 10, 25), " test ", " Some description ");

		// Act
		await _toDoItemService.UpdateAsync(itemToUpdate);

		// Assert
		_toDoItemRepository.Verify(r => r.UpdateAsync(
			It.Is<ToDoItem>(x => x.Title == "test" && x.Description == "Some description")), Times.Once);
	}

	[Fact]
	public async Task DeleteAsync_WhenCalled_ShouldCallRepositoryWithCorrectId()
	{
		// Arrange
		Guid id = Guid.NewGuid();

		// Act
		await _toDoItemService.DeleteAsync(id);

		// Assert
		_toDoItemRepository.Verify(r => r.DeleteAsync(id), Times.Once);
	}

	[Fact]
	public async Task ExistsAsync_WhenItemExists_ShouldReturnTrue()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		_toDoItemRepository.Setup(r => r.ExistsAsync(id)).ReturnsAsync(true);

		// Act
		bool result = await _toDoItemService.ExistsAsync(id);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public async Task ExistsAsync_WhenItemDoesNotExist_ShouldReturnFalse()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		_toDoItemRepository.Setup(r => r.ExistsAsync(id)).ReturnsAsync(false);

		// Act
		bool result = await _toDoItemService.ExistsAsync(id);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public async Task MarkAsDone_WhenCalled_ShouldSetCompletionPercentageTo100()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		ToDoItem toDoItem = CreateToDoItem(id, new DateTime(2025, 10, 25), "Test Title", "Desc", 0);
		_toDoItemRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(toDoItem);

		// Act
		await _toDoItemService.MarkAsDone(id);

		// Assert
		Assert.Equal(100, toDoItem.CompletionPercentage);
		_toDoItemRepository.Verify(r => r.UpdateAsync(toDoItem), Times.Once);
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(101)]
	[InlineData(-50)]
	[InlineData(150)]
	public async Task SetCompletionPercentageAsync_WhenInvalidPercentage_ShouldThrowArgumentOutOfRangeException(
		int invalidPercentage)
	{
		// Arrange
		Guid id = Guid.NewGuid();
		ToDoItem toDoItem = CreateToDoItem(id, new DateTime(2025, 10, 25), "Test Title", "Desc", 0);
		_toDoItemRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(toDoItem);

		// Act
		Func<Task> act = async () => await _toDoItemService.SetCompletionPercentageAsync(id, invalidPercentage);

		// Assert
		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(act);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(50)]
	[InlineData(100)]
	public async Task SetCompletionPercentageAsync_WhenValidPercentage_ShouldSetCompletionPercentage(
		int validPercentage)
	{
		// Arrange
		Guid id = Guid.NewGuid();
		ToDoItem toDoItem = CreateToDoItem(id, new DateTime(2025, 10, 25), "Test Title", "Desc", 0);
		_toDoItemRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(toDoItem);

		// Act
		await _toDoItemService.SetCompletionPercentageAsync(id, validPercentage);

		// Assert
		Assert.Equal(validPercentage, toDoItem.CompletionPercentage);
		_toDoItemRepository.Verify(r => r.UpdateAsync(toDoItem), Times.Once);
	}

	[Theory]
	[InlineData(IncomingPeriod.today)]
	[InlineData(IncomingPeriod.tomorrow)]
	[InlineData(IncomingPeriod.week)]
	public async Task GetIncomingAsync_WhenValidPeriod_ShouldReturnItemsFromRepository(IncomingPeriod period)
	{
		// Arrange
		List<ToDoItem> expectedItems = new()
		{
			CreateToDoItem(Guid.NewGuid(), new DateTime(2025, 10, 25), "Test", "Desc", 0)
		};

		_toDoItemRepository.Setup(r => r.GetByExpirationDateRangeAsync(
			It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(expectedItems);

		// Act
		IEnumerable<ToDoItem> result = await _toDoItemService.GetIncomingAsync(period);

		// Assert
		Assert.Equal(expectedItems, result);
		_toDoItemRepository.Verify(r => r.GetByExpirationDateRangeAsync(
			It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
	}

	[Fact]
	public async Task GetIncomingAsync_WhenInvalidPeriod_ShouldThrowArgumentOutOfRangeException()
	{
		// Arrange
		IncomingPeriod invalidValue = (IncomingPeriod)999;

		// Act
		Func<Task> act = async () => await _toDoItemService.GetIncomingAsync(invalidValue);

		// Assert
		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(act);
	}
}