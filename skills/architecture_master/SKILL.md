---
name: Architecture Master (API/BLL/DAL)
description: Master the classic 3-layer architecture in .NET, ensuring perfect separation between data access, business logic, and the presentation layer (API).
---

# Architecture Master: 3-Layer Pattern

This skill guides you through implementing a robust separation of concerns, which is essential for maintainable projects like tele-psychology platforms.

## 1. Structure & Layering
Organize the solution into three distinct projects to strictly enforce dependencies.

- **DAL (Data Access Layer)**: Only knows about entities and the database.
- **BLL (Business Logic Layer)**: Only knows about the DAL (via interfaces) and DTOs.
- **API (Web Layer)**: Only knows about the BLL (via interfaces).

### Dependency Flow
`API` → `BLL` → `DAL`

## 2. DAL: Data Access Layer
Contains the DbContext, Entities, and Repositories.

```csharp
// DAL/Repositories/IPatientRepository.cs
public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id);
    Task AddAsync(Patient patient);
}

// DAL/AppDbContext.cs
public class AppDbContext : DbContext { ... }
```

## 3. BLL: Business Logic Layer
Contains Services that coordinate business rules and use the DAL. It should strictly return and accept DTOs.

```csharp
// BLL/Services/PatientService.cs
public class PatientService : IPatientService
{
    private readonly IPatientRepository _repo;
    private readonly IMapper _mapper;

    public PatientService(IPatientRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<PatientDto> GetPatient(Guid id)
    {
        var patient = await _repo.GetByIdAsync(id);
        if (patient == null) throw new BusinessException("Patient not found");
        return _mapper.Map<PatientDto>(patient);
    }
}
```

## 4. API: Presentation Layer
Handels HTTP requests, authentication, and maps inputs to BLL calls.

```csharp
// API/Controllers/PatientsController.cs
[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _service;

    public PatientsController(IPatientService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PatientDto>> Get(Guid id)
    {
        var result = await _service.GetPatient(id);
        return Ok(result);
    }
}
```

## 5. Dependency Injection Registration
Configure all layers in the API's `Program.cs`.

```csharp
// API/Program.cs
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IPatientService, PatientService>();
```

## Best Practices
- **Never expose Entities to the UI**: Always use DTOs to avoid leaking database schema and prevent over-posting attacks.
- **Interfaces for Decoupling**: Define interfaces in the layer that *uses* them or in a shared `Core` project to allow easy mocking in unit tests.
- **Avoid "Fat" Controllers**: Controllers should only handle request parsing and returning appropriate HTTP statuses. Business logic belongs in the BLL.
- **Centralized Mapping**: Use AutoMapper or similar tools to manage the DTO <=> Entity conversion.
