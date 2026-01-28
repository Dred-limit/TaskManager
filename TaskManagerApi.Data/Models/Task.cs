namespace TaskManagerApi.Models;

public class TodoTask
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? Deadline { get; set; }

    public TodoTaskStatus Status { get; set; } = TodoTaskStatus.Todo;

    public int? UserId { get; set; }
    public User? User { get; set; }
}
