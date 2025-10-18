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

	public async Task<ToDoItem> GetByIdAsync(Guid id)
		=> (await _context.ToDoItems.FindAsync(id))!;

	public async Task<IEnumerable<ToDoItem>> GetAllAsync()
		=> await _context.ToDoItems.ToListAsync();

	public async Task AddAsync(ToDoItem item)
	{
		await _context.ToDoItems.AddAsync(item);
		await _context.SaveChangesAsync();
	}

	public async Task UpdateAsync(ToDoItem item)
	{
		_context.ToDoItems.Update(item);
		await _context.SaveChangesAsync();
	}

	public async Task DeleteAsync(Guid id)
	{
		ToDoItem item = (await _context.ToDoItems.FindAsync(id))!;
		_context.ToDoItems.Remove(item);
		await _context.SaveChangesAsync();
	}

	public async Task<bool> ExistsAsync(Guid id)
		=> await _context.ToDoItems.AnyAsync(x => x.Id == id);
}