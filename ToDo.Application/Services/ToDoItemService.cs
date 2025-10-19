using ToDo.Application.Interfaces.Repositories;
using ToDo.Application.Interfaces.Services;
using ToDo.Domain.Models;
using ToDo.Domain.Models.Enums;

namespace ToDo.Application.Services;

public class ToDoItemService : IToDoItemService
{
	private readonly IToDoItemRepository _toDoItemRepository;

	public ToDoItemService(IToDoItemRepository toDoItemRepository)
	{
		_toDoItemRepository = toDoItemRepository;
	}

	public async Task<ToDoItem?> GetByIdAsync(Guid id)
		=> await _toDoItemRepository.GetByIdAsync(id);

	public async Task<IEnumerable<ToDoItem>> GetAllAsync()
		=> await _toDoItemRepository.GetAllAsync();

	public async Task<Guid> AddAsync(ToDoItem toDoItem)
	{
		toDoItem.Normalize();
		return await _toDoItemRepository.AddAsync(toDoItem);
	}

	public async Task UpdateAsync(ToDoItem toDoItem)
	{
		toDoItem.Normalize();
		await _toDoItemRepository.UpdateAsync(toDoItem);
	}

	public async Task DeleteAsync(Guid id)
		=> await _toDoItemRepository.DeleteAsync(id);

	public async Task<bool> ExistsAsync(Guid id)
		=> await _toDoItemRepository.ExistsAsync(id);

	public async Task MarkAsDone(Guid id)
	{
		ToDoItem toDoItem = (await _toDoItemRepository.GetByIdAsync(id))!;
		toDoItem.MarkAsDone();
		await _toDoItemRepository.UpdateAsync(toDoItem);
	}

	public async Task SetCompletionPercentageAsync(
		Guid id,
		int percentage)
	{
		ToDoItem toDoItem = (await _toDoItemRepository.GetByIdAsync(id))!;
		toDoItem.SetCompletionPercentage(percentage);
		await _toDoItemRepository.UpdateAsync(toDoItem);
	}

	public async Task<IEnumerable<ToDoItem>> GetIncomingAsync(IncomingPeriod incomingPeriod)
	{
		DateTime utcNow = DateTime.UtcNow;
		DateTime utcNowDate = DateTime.UtcNow.Date;
		DateTime startDate;
		DateTime endDate;

		switch (incomingPeriod)
		{
			case IncomingPeriod.today:
				startDate = utcNow;
				endDate = utcNowDate.AddDays(1);
				break;
			case IncomingPeriod.tomorrow:
				startDate = utcNowDate.AddDays(1);
				endDate = utcNowDate.AddDays(2);
				break;
			case IncomingPeriod.week:
				startDate = utcNowDate;
				endDate = utcNowDate.AddDays(7);
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(incomingPeriod));
		}

		return await _toDoItemRepository.GetByExpirationDateRangeAsync(startDate, endDate);
	}
}