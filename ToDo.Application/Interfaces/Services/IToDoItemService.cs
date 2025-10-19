using ToDo.Domain.Models;
using ToDo.Domain.Models.Enums;

namespace ToDo.Application.Interfaces.Services;

public interface IToDoItemService
{
	Task<ToDoItem?> GetByIdAsync(Guid id);
	Task<IEnumerable<ToDoItem>> GetAllAsync();
	Task<Guid> AddAsync(ToDoItem toDoItem);
	Task UpdateAsync(ToDoItem toDoItem);
	Task DeleteAsync(Guid id);
	Task<bool> ExistsAsync(Guid id);
	Task MarkAsDone(Guid id);

	Task SetCompletionPercentageAsync(
		Guid id,
		int percentage);

	Task<IEnumerable<ToDoItem>> GetIncomingAsync(IncomingPeriod incomingPeriod);
}