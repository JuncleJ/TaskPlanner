namespace TaskPlanner.Models;
using Microsoft.AspNetCore.Identity;

public class TaskItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DueAt { get; set; }

    public bool IsCompleted { get; set; } = false;

    public bool IsDeleted { get; set; } = false;

    public string? UserId { get; set; }

    public IdentityUser? User { get; set; }
}