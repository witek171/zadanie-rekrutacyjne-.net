using System.ComponentModel.DataAnnotations;

namespace ToDo.Dtos;

public class ToDoItemRequest
{
	[Required]
	[DataType(DataType.DateTime)]
	public DateTime ExpirationDate { get; init; }

	[Required] [MaxLength(200)] public string Title { get; init; }
	[Required] [MaxLength(2000)] public string Description { get; init; }

	public ToDoItemRequest(
		DateTime expirationDate,
		string title,
		string description)
	{
		ExpirationDate = expirationDate;
		Title = title;
		Description = description;
	}
}