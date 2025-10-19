using Microsoft.EntityFrameworkCore;
using ToDo.Domain.Models;

namespace ToDo.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
	}

	public DbSet<ToDoItem> ToDoItems { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<ToDoItem>(entity =>
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.Title)
				.IsRequired()
				.HasMaxLength(200);

			entity.Property(e => e.Description)
				.IsRequired()
				.HasMaxLength(2000);

			entity.Property(e => e.CompletionPercentage)
				.IsRequired()
				.HasDefaultValue(0);

			entity.Property(e => e.ExpirationDate)
				.IsRequired();

			entity.ToTable(t => t.HasCheckConstraint(
				"CK_ToDoItem_CompletionPercentage",
				"\"CompletionPercentage\" >= 0 AND \"CompletionPercentage\" <= 100"));
		});
	}
}