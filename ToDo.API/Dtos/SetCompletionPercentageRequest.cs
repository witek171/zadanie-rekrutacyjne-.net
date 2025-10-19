using System.ComponentModel.DataAnnotations;

namespace ToDo.Dtos;

public class SetCompletionPercentageRequest
{
	[Required] [Range(0, 100)] public int Percentage { get; init; }
}