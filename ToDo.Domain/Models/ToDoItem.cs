using System.ComponentModel.DataAnnotations.Schema;

namespace ToDo.Domain.Models;

[Table("ToDoItems")]
public class ToDoItem
{
	[Column("Id")] public Guid Id { get; private set; }
	[Column("ExpirationDate")] public DateTime ExpirationDate { get; private set; }
	[Column("Title")] public string Title { get; private set; }
	[Column("Description")] public string Description { get; private set; }
	[Column("CompletionPercentage")] public int CompletionPercentage { get; private set; }

	public ToDoItem(
		DateTime expirationDate,
		string title,
		string description)
	{
		Id = Guid.NewGuid();
		ExpirationDate = expirationDate;
		Title = title;
		Description = description;
		CompletionPercentage = 0;
	}

	public void Normalize()
	{
		Title = Title.Trim();
		Description = Description.Trim();
	}

	public void MarkAsDone()
		// mozna by dodac rzucanie wyjatku gdy CompletionPercentage jest juz ustawione na 100
		=> CompletionPercentage = 100;

	public void Update(
		DateTime expirationDate,
		string title,
		string description)
	{
		ExpirationDate = expirationDate;
		Title = title;
		Description = description;
	}

	public void SetCompletionPercentage(int completionPercentage)
	{
		// mozna by dodac rzucanie wyjatku gdy CompletionPercentage jest juz ustawione na podany procent 
		if (completionPercentage is < 0 or > 100)
			throw new ArgumentOutOfRangeException(nameof(completionPercentage),
				"Percentage must be between 0 and 100");

		CompletionPercentage = completionPercentage;
	}
}