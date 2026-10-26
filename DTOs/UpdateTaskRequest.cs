using System.ComponentModel.DataAnnotations;

namespace TaskPlanner.DTOs;

public class UpdateTaskRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? DueAt { get; set; }

    public bool IsCompleted { get; set; }
}