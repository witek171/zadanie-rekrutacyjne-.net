using ToDo.Domain.Models;

namespace ToDo.Application.Interfaces.Repositories;

public interface IToDoItemRepository
{
	Task<ToDoItem> GetByIdAsync(Guid id);
	Task<IEnumerable<ToDoItem>> GetAllAsync();
	Task AddAsync(ToDoItem toDoItem);
	Task UpdateAsync(ToDoItem toDoItem);
	Task DeleteAsync(Guid id);
	Task<bool> ExistsAsync(Guid id);
}