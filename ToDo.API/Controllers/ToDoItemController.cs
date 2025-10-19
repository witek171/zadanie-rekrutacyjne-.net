using Microsoft.AspNetCore.Mvc;
using ToDo.Application.Interfaces.Services;
using ToDo.Domain.Models;
using ToDo.Domain.Models.Enums;
using ToDo.Dtos;
using ToDo.Mappers;

namespace ToDo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ToDoItemController : ControllerBase
{
	private readonly IToDoItemService _toDoItemService;

	public ToDoItemController(IToDoItemService toDoItemService)
	{
		_toDoItemService = toDoItemService;
	}

	[HttpGet]
	public async Task<ActionResult<List<ToDoItemResponse>>> GetAll()
	{
		IEnumerable<ToDoItem> toDoItems = await _toDoItemService.GetAllAsync();
		List<ToDoItemResponse> responses = toDoItems
			.Select(ToDoItemMapper.EntityToResponse)
			.ToList();
		return Ok(responses);
	}

	[HttpGet("{id:guid}")]
	public async Task<ActionResult<ToDoItemResponse>> GetById(Guid id)
	{
		ToDoItem? toDoItem = await _toDoItemService.GetByIdAsync(id);
		if (toDoItem == null)
			return NotFound();
		ToDoItemResponse response = ToDoItemMapper.EntityToResponse(toDoItem);
		return Ok(response);
	}

	[HttpPost]
	public async Task<ActionResult<Guid>> Create([FromBody] ToDoItemRequest toDoItemRequest)
	{
		ToDoItem toDoItem = ToDoItemMapper.RequestToEntity(toDoItemRequest);
		Guid id = await _toDoItemService.AddAsync(toDoItem);
		return CreatedAtAction(nameof(Create), id);
	}

	[HttpPut("{id:guid}/setCompletionPercentage")]
	public async Task<ActionResult> SetCompletionPercentage(
		Guid id,
		[FromBody] SetCompletionPercentageRequest request)
	{
		if (!await _toDoItemService.ExistsAsync(id))
			return NotFound();
		await _toDoItemService.SetCompletionPercentageAsync(id, request.Percentage);
		return NoContent();
	}

	// dla uproszczenia zwracam rowniez zadania ktore maja ukonczenie 100% 
	[HttpGet("incoming")]
	public async Task<ActionResult<List<ToDoItemResponse>>> GetIncoming(
		[FromQuery] IncomingPeriod incomingPeriod)
	{
		IEnumerable<ToDoItem> toDoItems = await _toDoItemService.GetIncomingAsync(incomingPeriod);
		List<ToDoItemResponse> responses = toDoItems
			.Select(ToDoItemMapper.EntityToResponse)
			.ToList();
		return Ok(responses);
	}

	[HttpPut("{id:guid}")]
	public async Task<ActionResult> Update(
		Guid id,
		[FromBody] ToDoItemRequest toDoItemRequest)
	{
		ToDoItem? toDoItem = await _toDoItemService.GetByIdAsync(id);
		if (toDoItem == null)
			return NotFound();
		ToDoItemMapper.UpdateEntityFromRequest(toDoItem, toDoItemRequest);
		await _toDoItemService.UpdateAsync(toDoItem);
		return NoContent();
	}

	[HttpPut("{id:guid}/markAsDone")]
	public async Task<ActionResult> MarkAsDone(Guid id)
	{
		if (!await _toDoItemService.ExistsAsync(id))
			return NotFound();
		await _toDoItemService.MarkAsDone(id);
		return NoContent();
	}

	[HttpDelete("{id:guid}")]
	public async Task<ActionResult> Delete(Guid id)
	{
		if (!await _toDoItemService.ExistsAsync(id))
			return NotFound();
		await _toDoItemService.DeleteAsync(id);
		return NoContent();
	}
}