using System.ComponentModel.DataAnnotations;

namespace TodoRestApi.Models;

public class TodoItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Todo { get; set; } = null!;

    public bool IsCompleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

}