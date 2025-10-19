using ToDo.Domain.Models;
using ToDo.Dtos;

namespace ToDo.Mappers;

public static class ToDoItemMapper
{
	public static ToDoItem RequestToEntity(ToDoItemRequest toDoItemRequest)
		=> new(
			toDoItemRequest.ExpirationDate,
			toDoItemRequest.Title,
			toDoItemRequest.Description);

	public static ToDoItemResponse EntityToResponse(ToDoItem toDoItem)
		=> new(
			toDoItem.Id,
			toDoItem.ExpirationDate,
			toDoItem.Title,
			toDoItem.Description,
			toDoItem.CompletionPercentage);

	public static void UpdateEntityFromRequest(
		ToDoItem toDoItem,
		ToDoItemRequest toDoItemRequest)
	{
		toDoItem.Update(
			toDoItemRequest.ExpirationDate,
			toDoItemRequest.Title,
			toDoItemRequest.Description);
	}
}