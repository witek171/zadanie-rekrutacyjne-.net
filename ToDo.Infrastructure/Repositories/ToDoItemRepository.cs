using Microsoft.EntityFrameworkCore;
using ToDo.Application.Interfaces.Repositories;
using ToDo.Domain.Models;
using ToDo.Infrastructure.Data;

namespace ToDo.Infrastructure.Repositories;

public class ToDoItemRepository : IToDoItemRepository
{
	private readonly ApplicationDbContext _context;

	public ToDoItemRepository(ApplicationDbContext context)
	{
		_context = context;
	}

	public async Task<ToDoItem?> GetByIdAsync(Guid id)
		=> await _context.ToDoItems.FindAsync(id);

	public async Task<IEnumerable<ToDoItem>> GetAllAsync()
		=> await _context.ToDoItems.ToListAsync();

	public async Task<Guid> AddAsync(ToDoItem toDoItem)
	{
		await _context.ToDoItems.AddAsync(toDoItem);
		await _context.SaveChangesAsync();
		return toDoItem.Id;
	}

	public async Task UpdateAsync(ToDoItem toDoItem)
	{
		_context.ToDoItems.Update(toDoItem);
		await _context.SaveChangesAsync();
	}

	public async Task DeleteAsync(Guid id)
	{
		ToDoItem toDoItem = (await _context.ToDoItems.FindAsync(id))!;
		_context.ToDoItems.Remove(toDoItem);
		await _context.SaveChangesAsync();
	}

	public async Task<bool> ExistsAsync(Guid id)
		=> await _context.ToDoItems.AnyAsync(x => x.Id == id);

	public async Task<IEnumerable<ToDoItem>> GetByExpirationDateRangeAsync(
		DateTime startDate,
		DateTime endDate)
		=> await _context.ToDoItems
			.Where(x => x.ExpirationDate >= startDate && x.ExpirationDate < endDate)
			.OrderBy(x => x.ExpirationDate)
			.ToListAsync();
}