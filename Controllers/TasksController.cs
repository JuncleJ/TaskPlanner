using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TaskPlanner.Data;
using TaskPlanner.DTOs;
using TaskPlanner.Models;
using Microsoft.AspNetCore.Identity;

namespace TaskPlanner.Controllers;

[Authorize]
[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private string CurrentUserId => _userManager.GetUserId(User) ?? throw new UnauthorizedAccessException();

    public TasksController(AppDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? sort)
    {
        IQueryable<TaskItem> query = _db.Tasks
            .AsNoTracking()
            .Where(t => !t.IsDeleted && t.UserId == CurrentUserId);

        // Поиск по названию
        if (!string.IsNullOrWhiteSpace(search))
        {
            string pattern = $"%{search.Trim()}%";

            query = query.Where(t =>
                EF.Functions.ILike(t.Title, pattern));
        }

        // Сортировка
        query = sort switch
        {
            "oldest" => query.OrderBy(t => t.CreatedAt),

            "due_asc" => query
                .OrderBy(t => t.DueAt == null)
                .ThenBy(t => t.DueAt),

            "due_desc" => query
                .OrderBy(t => t.DueAt == null)
                .ThenByDescending(t => t.DueAt),

            _ => query.OrderByDescending(t => t.CreatedAt)
        };

        var tasks = await query.ToListAsync();

        return Ok(tasks);
    }

    // GET /api/tasks/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await _db.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

        if (task is null)
            return NotFound();

        return Ok(task);
    }

    // POST /api/tasks
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("Title is required");

        var task = new TaskItem
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            UserId = CurrentUserId
        };

        _db.Tasks.Add(task);

        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = task.Id },
            task
        );
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("Title is required");

        var task = await _db.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted && t.UserId == CurrentUserId);

        if (task is null)
            return NotFound();

        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.DueAt = request.DueAt;
        task.IsCompleted = request.IsCompleted;

        await _db.SaveChangesAsync();

        return Ok(task);
    }

    // DELETE /api/tasks/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _db.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted && t.UserId == CurrentUserId);

        if (task is null)
            return NotFound();

        task.IsDeleted = true;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("trash")]
    public async Task<IActionResult> GetTrash()
    {
        var tasks = await _db.Tasks
            .AsNoTracking()
            .Where(t => t.IsDeleted && t.UserId == CurrentUserId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpPost("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id)
    {
        var task = await _db.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.IsDeleted && t.UserId == CurrentUserId);

        if (task is null)
            return NotFound();

        task.IsDeleted = false;

        await _db.SaveChangesAsync();

        return Ok(task);
    }

    [HttpDelete("trash/{id:int}")]
    public async Task<IActionResult> DeleteForever(int id)
    {
        var task = await _db.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.IsDeleted && t.UserId == CurrentUserId);

        if (task is null)
            return NotFound();

        _db.Tasks.Remove(task);

        await _db.SaveChangesAsync();

        return NoContent();
    }
}