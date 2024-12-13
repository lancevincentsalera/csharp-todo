using Microsoft.EntityFrameworkCore;
using TodoRestApi.Models;

namespace TodoRestApi.Data;

public class TodoListDbContext : DbContext
{
    public TodoListDbContext(DbContextOptions<TodoListDbContext> options) : base(options) { }
    public DbSet<TodoItem> TodoItems { get; set; } = null!;
}