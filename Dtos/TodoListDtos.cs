namespace TodoRestApi.Dtos;

public record TodoItemReadDto(int Id, string Todo, bool IsCompleted, DateTime CreatedAt, DateTime UpdatedAt);
public record TodoItemCreateDto(string Todo);
public record TodoItemUpdateDto(string Todo, bool IsCompleted);
