using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TaskManagerApi.Models;

namespace TaskManagerApi.Data.DTOs;

public class CreateTaskDto
{
    [Required(ErrorMessage = "Заголовок обязателен")]
    [MinLength(3, ErrorMessage = "Минимум 3 символа")]
    [MaxLength(60, ErrorMessage = "Максимум 60 символов")]
    public string Title { get; set; }

    [MaxLength(500, ErrorMessage = "Максимум 500 символов")]
    public string? Description { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Некорректный ID пользователя")]
    public int UserId { get; set; }
}

public class UpdateTaskDto
{
    [MinLength(3), MaxLength(60)]
    public string? Title { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue)]
    public int? UserId { get; set; }

    public TodoTaskStatus? Status { get; set; }
}

public class TaskResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public TodoTaskStatus Status { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public UserDto? User { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}