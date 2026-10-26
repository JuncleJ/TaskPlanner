using System.ComponentModel.DataAnnotations;

namespace TaskPlanner.DTOs;

public class CreateTaskRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
}