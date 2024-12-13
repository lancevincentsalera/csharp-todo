using Microsoft.EntityFrameworkCore;
using TodoRestApi.Data;
using TodoRestApi.Dtos;
using TodoRestApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// builder.Services.AddEndpointsApiExplorer();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddDbContext<TodoListDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
// }

app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<TodoListDbContext>();
db.Database.Migrate();


/* Minimal APIs */
/******************/

#region GET ALL TODO ITEMS
// GET ALL TODO ITEM IN THE TODO LIST
app.MapGet("api/todolist", async (TodoListDbContext db) =>
    await db.TodoItems.Select(todo => new TodoItemReadDto(todo.Id, todo.Todo, todo.IsCompleted, todo.CreatedAt, todo.UpdatedAt)).ToListAsync()
);
#endregion


#region ADD TODO ITEM
// ADD A TODO ITEM
app.MapPost("/api/add/todoitem", async (TodoListDbContext db, TodoItemCreateDto input) =>
{
    var todo = new TodoItem
    {
        Todo = input.Todo,
    };
    db.TodoItems.Add(todo);
    await db.SaveChangesAsync();
    var todoDto = new TodoItemReadDto(todo.Id, todo.Todo, todo.IsCompleted, todo.CreatedAt, todo.UpdatedAt);
    return Results.Created($"api/add/todoitem/{todo.Id}", todoDto);
});
#endregion


#region UPDATE TODO ITEM
// UPDATE THE SPECIFIED TODO ITEM GIVEN THE ID
app.MapPut("/api/update/todoitem/{id}", async (TodoListDbContext db, int id, TodoItemUpdateDto todoUpdate) =>
{
    var todo = await db.TodoItems.FindAsync(id);
    if (todo is null) return Results.NotFound($"Item with id: {id} does not exist");

    todo.Todo = todoUpdate.Todo;
    todo.IsCompleted = todoUpdate.IsCompleted;
    todo.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();

    return Results.NoContent();
});
#endregion


#region DELETE COMPLETED ITEMS
// CLEAR ALL COMPLETED ITEMS
app.MapDelete("/api/delete/completed", async (TodoListDbContext db) =>
{
    var todos = await db.TodoItems.Where(todo => todo.IsCompleted).ToListAsync();
    if (todos.Count == 0)
    {
        return Results.NotFound("No completed items to delete.");
    }

    db.TodoItems.RemoveRange(todos);

    await db.SaveChangesAsync();

    return Results.Ok("All completed items have been deleted.");
});
#endregion


app.Run();

