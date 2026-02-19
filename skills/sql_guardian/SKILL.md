---
name: SQL Guardian (.NET + SQL Server)
description: Expert data persistence skill focused on Entity Framework Core, SQL Server optimization, and reliable migration strategies.
---

# SQL Guardian Skill: Master Data Persistence

Efficiently managing data in a .NET + SQL Server environment is critical for application performance and reliability.

## 1. EF Core Configuration (Fluent API)
Use Fluent API instead of Data Annotations for cleaner DTOs and more powerful configuration control.

```csharp
// Data/AppDbContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Patient>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
        entity.HasIndex(e => e.Email).IsUnique();
        
        // Shadow property for audit
        entity.Property<DateTime>("LastUpdated");
    });
}
```

## 2. Performance: NoTracking and Projections
Always optimize read-only queries to reduce memory footprint and execution time.

```csharp
// Use AsNoTracking for read scenarios
var therapists = await _db.Therapists
    .AsNoTracking()
    .Where(t => t.IsActive)
    .Select(t => new TherapistListItemDto { // Projection
        Id = t.Id,
        Name = t.FullName,
        Specialty = t.Specialization
    })
    .ToListAsync();
```

## 3. SQL Server Optimization
- **Indexes**: Ensure frequently filtered columns have appropriate indexes.
- **Transactions**: Use `IDbContextTransaction` for operations involving multiple changes.
- **Connection Resiliency**: Enable execution strategy in `DbContext` setup.

```csharp
// Program.cs
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlServerOptions =>
    {
        sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));
```

## 4. Migration Management
Keep your database schema in sync with your models reliably.
- **Rule 1**: Always name migrations descriptively (`AddPatientTable`, `AddTherapistIndex`).
- **Rule 2**: Check generated SQL before applying (`dotnet ef migrations script`).
- **Rule 3**: Handle data seeding via migrations or a dedicated service.

## 5. Repository & Unit of Work (Lite)
For larger projects, abstract the data access to keep features clean and testable.

```csharp
public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id);
    Task AddAsync(Patient patient);
}
```

## Best Practices
- **Never store plain passwords**: Use hashing (BCrypt/identity).
- **Hard Delete vs Soft Delete**: Prefer soft deletes (IsDeleted flag) for healthcare data.
- **Batching**: EF Core 7+ supports `ExecuteUpdateAsync` and `ExecuteDeleteAsync` for high-performance batch operations.
