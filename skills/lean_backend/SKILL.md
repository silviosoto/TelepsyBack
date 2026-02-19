---
name: Lean Backend (.NET)
description: A skill focused on building high-performance, minimalist .NET Web APIs using minimal dependencies and the 'Minimal API' style.
---

# Lean Backend Skill: .NET Minimal APIs

This skill focuses on creating efficient, lightweight backends using .NET 8/9 Minimal APIs. The goal is to reduce boilerplate while maintaining scalability.

## 1. Project Structure (Vertical Slice Lite)
Organize features by domain functionality rather than technical layers (Controllers/Services/Repositories).

```text
/Features
  /Users
    CreateUser.cs
    GetUser.cs
  /Products
    GetProduct.cs
/Shared
  /Data
  /Extensions
Program.cs
```

## 2. Minimal API Setup (Program.cs)
Keep `Program.cs` clean. Use extension methods to register services and endpoints.

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options => ...);

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map endpoints
app.MapUserEndpoints();

app.Run();
```

## 3. Endpoint Definition
Define endpoints directly or use static extension methods to group them.

```csharp
// Features/Users/UserEndpoints.cs
public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users");

        group.MapGet("/{id}", GetUser);
        group.MapPost("/", CreateUser);
    }

    static async Task<IResult> GetUser(int id, AppDbContext db)
    {
        return await db.Users.FindAsync(id)
            is User user
                ? Results.Ok(user)
                : Results.NotFound();
    }

    static async Task<IResult> CreateUser(UserDto dto, AppDbContext db)
    {
        var user = new User { Name = dto.Name };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return Results.Created($"/api/users/{user.Id}", user);
    }
}
```

## 4. Validation (FluentValidation)
Use `FluentValidation` for request validation instead of Data Annotations to keep DTOs clean.

```csharp
public class CreateUserValidator : AbstractValidator<UserDto>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Email).EmailAddress();
    }
}
```
*Tip: Use a filter or middleware to auto-validate requests.*

## 5. Performance Tips
- **AsNoTracking**: Use `.AsNoTracking()` for read-only queries in EF Core.
- **Compiled Models**: Use compiled models for EF Core startup performance.
- **AOT**: Consider Native AOT compilation for microservices if cold start is critical.

## Core Principles
1. **Less Code**: Use records for DTOs.
2. **explicit dependencies**: Inject only what you need into the endpoint handler.
3. **No Controllers**: Stick to `IResult` and static methods.
