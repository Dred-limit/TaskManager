using TaskManagerApi.Data;
using TaskManagerApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=taskmanager.db"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
        var user = new User
        {
            Name = "Тестовый Пользователь",
            Email = "test@example.com"
        };

        db.Users.Add(user);
        db.SaveChanges();

        var tasks = new List<TodoTask>
        {
            new TodoTask
            {
                Title = "Подготовиться к экзамену",
                UserId = user.Id,
                Status = TodoTaskStatus.Todo,
                CreatedDate = DateTime.UtcNow
            }
        };

        db.Tasks.AddRange(tasks);
        db.SaveChanges();

    }
}

app.MapGet("/tasks", async (AppDbContext db) =>
{
    var tasks = await db.Tasks
        .AsNoTracking()
        .Select(task => new
        {
            task.Id,
            task.Title,
            task.CreatedDate,
            task.Status,
        }).ToListAsync();

    return tasks;
});

app.MapGet("/tasks/{id}", async (int id, AppDbContext db) =>
{
    if (id <= 0)
    {
        return Results.BadRequest("ID must be a positive number"); 
    }

    var task = await db.Tasks
        .Include(t => t.User)
        .AsNoTracking()
        .FirstOrDefaultAsync(t => t.Id == id);

    if (task == null)
    {
        return Results.NotFound($"Task with ID {id} not found");
    }

    return Results.Ok(new
    {
        task.Id,
        task.Title,
        task.Description,
        task.CreatedDate,
        task.Status,

        User = task.User == null ? null : new
        {
            task.User.Id,
            task.User.Name,
            task.User.Email
        }
    });
});

app.MapPost("/tasks", async (TodoTask newTask, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(newTask.Title))
    {
        return Results.BadRequest("Task title is required");
    }

    if (newTask.UserId <= 0)
    {
        return Results.BadRequest("Invalid user ID");
    }

    // есть ли пользователь?
    var userExists = await db.Users
        .AsNoTracking()
        .AnyAsync(u => u.Id == newTask.UserId);
    
    if (!userExists)
    {
        return Results.BadRequest($"User with ID {newTask.UserId} not found");
    }

    newTask.CreatedDate = DateTime.UtcNow;

    if (!Enum.IsDefined(typeof(TodoTaskStatus), newTask.Status))
    {
        newTask.Status = TodoTaskStatus.Todo;
    }

    db.Tasks.Add(newTask);

    try
    {
        return Results.Created($"/tasks/{newTask.Id}", new
        {
            newTask.Id,
            newTask.Title,
            newTask.Description,
            newTask.Status,
            newTask.UserId,
            newTask.CreatedDate,
            Message = "The task was created successfully."
        });
    }
    catch (DbUpdateException ex)
    {
        return Results.Problem(
            title: "Ошибка при сохранении задачи",
            detail: ex.InnerException?.Message ?? ex.Message,
            statusCode: 500
            );
    }
});

app.MapPut("/tasks/{id}", async (int id, TodoTask updatedTask, AppDbContext db) =>
{
    var existingTask = await db.Tasks.FindAsync(id);
    if (existingTask == null)
    {
        return Results.NotFound($"Task with ID {id} nor found"); 
    }

    if(string.IsNullOrWhiteSpace(updatedTask.Title))
    {
        return Results.BadRequest("Task title is required");
    }

    if (updatedTask.UserId != existingTask.UserId)
    {
        var userExists = await db.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == updatedTask.UserId);

        if (!userExists)
        {
            return Results.BadRequest($"User with ID {updatedTask.UserId} not found");
        }
    }

    existingTask.Title = updatedTask.Title;
    existingTask.Description = updatedTask.Description;
    existingTask.Status = updatedTask.Status;
    existingTask.UserId = updatedTask.UserId;

    try
    {
        await db.SaveChangesAsync();

        return Results.Ok(new
        {
            existingTask.Id,
            existingTask.Title,
            existingTask.Description,
            existingTask.Status,
            existingTask.CreatedDate,
            existingTask.UserId,
            Message = "Task updated successfully."
        });
    }
    catch (DbUpdateException ex)
    {
        return Results.Problem(
            title: "Ошибка при обновлении задачи",
            detail: ex.InnerException?.Message ?? ex.Message,
            statusCode: 500
            );
    }
});

app.MapDelete("/tasks/{id}", async (int id, AppDbContext db) =>
{
    var task = await db.Tasks.FindAsync(id);

    if (task == null)
    {
        return Results.NotFound($"Task with ID {id} nor found");
    }

    db.Tasks.Remove(task);

    try
    {
        await db.SaveChangesAsync();

        return Results.Ok(new
        {
            Message = $"Task with ID {id} deleted successfully"
        });
    }
    catch (DbUpdateException ex)
    {
        return Results.Problem(
            title: "Ошибка при удалении задачи",
            detail: ex.InnerException?.Message ?? ex.Message,
            statusCode: 500
        );
    }
});

app.Run();
