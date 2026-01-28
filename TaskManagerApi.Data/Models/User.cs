using Microsoft.AspNetCore.Identity;

namespace TaskManagerApi.Models;

public class User : IdentityUser<int>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public ICollection<TodoTask> UserTasks { get; set; } = new List<TodoTask>();
}
