using System.ComponentModel.DataAnnotations;

namespace ToDo.Dtos;

public class ToDoItemResponse
{
	[Required] public Guid Id { get; init; }
	[Required] public DateTime ExpirationDate { get; init; }
	[Required] public string Title { get; init; }
	[Required] public string Description { get; init; }
	[Required] public int CompletionPercentage { get; init; }

	public ToDoItemResponse(
		Guid id,
		DateTime expirationDate,
		string title,
		string description,
		int completionPercentage)
	{
		Id = id;
		ExpirationDate = expirationDate;
		Title = title;
		Description = description;
		CompletionPercentage = completionPercentage;
	}
}