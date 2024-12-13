using Microsoft.EntityFrameworkCore;
using TodoRestApi.Data;
using TodoRestApi.Dtos;
using TodoRestApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration.AddUserSecrets<Program>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddDbContext<TodoListDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
).WithName("GetAllTodoItems")
.WithTags("TodoList")
.Produces<List<TodoItemReadDto>>(StatusCodes.Status200OK)
.WithDescription("Retrieves all Todo items.");
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
}).WithName("AddTodoItem")
.WithTags("TodoList")
.Produces<TodoItemReadDto>(StatusCodes.Status201Created)
.WithDescription("Adds a new Todo item.");
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
}).WithName("UpdateTodoItem")
.WithTags("TodoList")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound)
.WithDescription("Updates an existing Todo item by ID.");
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
}).WithName("DeleteCompletedItems")
.WithTags("TodoList")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.WithDescription("Deletes all completed Todo items.");
#endregion


app.Run();

