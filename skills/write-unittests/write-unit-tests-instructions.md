---
applyTo: '**'
---

# Testing Instructions and Patterns

## Comprehensive Testing Patterns and Guidelines

### Test Structure Pattern: Arrange-Act-Assert (AAA)

The **Arrange-Act-Assert** pattern is the fundamental structure for writing clear, maintainable unit tests. This pattern divides each test into three distinct sections:

#### 1. **Arrange** - Set up the test
- Create and configure test objects
- Initialize dependencies and mocks
- Set up test data and expected values
- Configure the system under test

#### 2. **Act** - Execute the behavior being tested
- Call the method or trigger the action being tested
- This should typically be a single line or operation
- Capture the result or exception if needed

#### 3. **Assert** - Verify the expected outcome
- Check that the actual result matches expectations
- Verify that dependencies were called correctly
- Validate side effects and state changes

#### AAA Pattern Examples

**Basic Service Method Test:**
```csharp
[Test]
public void CalculateTotal_WithValidItems_ReturnsCorrectSum()
{
    // Arrange
    var calculator = new PriceCalculator();
    var items = new List<Item>
    {
        new Item { Price = 10.00m },
        new Item { Price = 25.50m },
        new Item { Price = 5.75m }
    };
    var expectedTotal = 41.25m;

    // Act
    var actualTotal = calculator.CalculateTotal(items);

    // Assert
    Assert.That(actualTotal, Is.EqualTo(expectedTotal)); // Using constraint model
}
```

**Repository Test with Mocking:**
```csharp
[Test]
public async Task GetPaymentAsync_WithValidId_ReturnsPayment()
{
    // Arrange
    var paymentId = Guid.NewGuid();
    var expectedPayment = new Payment { Id = paymentId, Amount = 100.00m };
    var fakeRepository = A.Fake<IPaymentRepository>();
    var service = new PaymentService(fakeRepository);
    
    A.CallTo(() => fakeRepository.GetByIdAsync(paymentId))
        .Returns(Task.FromResult(expectedPayment));

    // Act
    var actualPayment = await service.GetPaymentAsync(paymentId);

    // Assert
    Assert.That(actualPayment, Is.Not.Null);                    // Constraint model
    Assert.That(actualPayment.Id, Is.EqualTo(paymentId));       // Constraint model
    Assert.That(actualPayment.Amount, Is.EqualTo(100.00m));     // Constraint model
}
```

**Exception Testing:**
Synchronous method
```csharp
[Test]
public void ProcessPayment_WithNullRequest_ThrowsArgumentNullException()
{
    // Arrange
    var paymentService = new PaymentService();
    PaymentRequest request = null;

    // Act & Assert (combined for exception testing)
    var exception = Assert.Throws<ArgumentNullException>(() => 
        paymentService.ProcessPayment(request));
    
    // Using constraint model for exception verification
    Assert.That(exception.ParamName, Is.EqualTo("request"));
}
```

Async Method Testing:
typically method will take a CancellationToken as parameter
```csharp
[Test]
public void MarkFileAsTransferredAsync_WithEmptyGuid_ThrowsArgumentException()
{
    // Arrange
    var emptyGuid = Guid.Empty;
    var blobPath = "test/path/file.txt";

    // Act & Assert
    var exception = Assert.ThrowsAsync<ArgumentException>(() =>
        fileService.MarkFileAsTransferredAsync(emptyGuid, blobPath));

    Assert.Multiple(() =>
    {
        Assert.That(exception.Message, Does.Contain("File Id cannot be empty"));
        Assert.That(exception.ParamName, Is.EqualTo("fileId"));
    });
}
```

**Async Method Testing:**
```csharp
[Test]
public async Task CreatePaymentAsync_WithValidData_CreatesAndReturnsPayment()
{
    // Arrange
    var fakeRepository = A.Fake<IPaymentRepository>();
    var service = new PaymentService(fakeRepository);
    var paymentData = new PaymentData 
    { 
        Amount = 50.00m, 
        Description = "Test Payment" 
    };
    var expectedId = Guid.NewGuid();
    
    A.CallTo(() => fakeRepository.CreateAsync(A<Payment>._))
        .Returns(Task.FromResult(expectedId));

    // Act
    var result = await service.CreatePaymentAsync(paymentData);

    // Assert
    Assert.That(result, Is.EqualTo(expectedId));
    A.CallTo(() => fakeRepository.CreateAsync(A<Payment>.That.Matches(p => 
        p.Amount == paymentData.Amount && 
        p.Description == paymentData.Description)))
        .MustHaveHappenedOnceExactly();
}
```

**CancellationToken Testing:**
```csharp
[Test]
public async Task ExecuteCustomActionAsync_WithValidRequest_ReturnsExpectedResponse()
{
    // Arrange
    var fakeCustomActionRepository = A.Fake<ICustomActionRepository>();
    var customActionService = new CustomActionService(fakeCustomActionRepository);
    var request = new CustomActionRequest 
    { 
        ActionName = "new_TestAction",
        InputParameters = new Dictionary<string, object> { { "param1", "value1" } }
    };
    var expectedResponse = new CustomActionResponse
    {
        ActionName = "new_TestAction",
        Success = true,
        OutputParameters = new Dictionary<string, object> { { "result", "success" } },
        ExecutionTimeMs = 200
    };
    var cancellationToken = new CancellationToken();

    A.CallTo(() => fakeCustomActionRepository.ExecuteCustomActionAsync(
        A<CustomActionRequest>.That.Matches(r => r.ActionName == request.ActionName),
        cancellationToken)).Returns(expectedResponse);

    // Act
    var result = await customActionService.ExecuteCustomActionAsync(request, cancellationToken);

    // Assert
    Assert.Multiple(() =>
    {
        Assert.That(result, Is.EqualTo(expectedResponse));
        Assert.That(result.ActionName, Is.EqualTo("new_TestAction"));
        Assert.That(result.Success, Is.True);
        Assert.That(result.OutputParameters.Count, Is.EqualTo(1));
        Assert.That(result.ExecutionTimeMs, Is.EqualTo(200));
    });

    A.CallTo(() => fakeCustomActionRepository.ExecuteCustomActionAsync(
        A<CustomActionRequest>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
}
```

**CancellationToken with Timeout Testing:**
```csharp
[Test]
public async Task ExecuteCustomActionAsync_WithCancellation_ThrowsOperationCancelledException()
{
    // Arrange
    var fakeCustomActionRepository = A.Fake<ICustomActionRepository>();
    var customActionService = new CustomActionService(fakeCustomActionRepository);
    var request = new CustomActionRequest { ActionName = "new_TestAction" };
    var cancellationTokenSource = new CancellationTokenSource();
    var cancellationToken = cancellationTokenSource.Token;

    A.CallTo(() => fakeCustomActionRepository.ExecuteCustomActionAsync(
        A<CustomActionRequest>._, A<CancellationToken>._))
        .Returns(Task.FromCanceled<CustomActionResponse>(cancellationToken));

    // Act & Assert
    cancellationTokenSource.Cancel();
    
    var exception = await Assert.ThrowsAsync<OperationCanceledException>(() => 
        customActionService.ExecuteCustomActionAsync(request, cancellationToken));
    
    Assert.That(exception.CancellationToken, Is.EqualTo(cancellationToken));
}
```

#### AAA Pattern Best Practices

1. **Clear Separation**: Use comments or blank lines to visually separate the three sections
2. **Single Act**: Each test should have only one Act section with one primary action
3. **Descriptive Arrange**: Make setup code clear and easy to understand
4. **Comprehensive Assert**: Verify all important aspects of the result using NUnit constraint model
5. **Test One Thing**: Each test should focus on one specific behavior or scenario
6. **Use Constraint Model**: Always prefer `Assert.That()` with constraints over classic assert methods

#### AAA Pattern in Different Test Types

**Domain Service Test:**
```csharp
[Test]
public void ValidatePaymentRequest_WithInvalidEmail_ReturnsFalse()
{
    // Arrange
    var validator = new PaymentRequestValidator();
    var request = new PaymentRequestDto
    {
        Amount = 100.00m,
        EmailAddress = "invalid-email", // Invalid email format
        FirstName = "John",
        LastName = "Doe"
    };

    // Act
    var isValid = validator.Validate(request);

    // Assert
    Assert.That(isValid, Is.False); // Constraint model preferred
}
```

**Integration Test:**
```csharp
[Test]
[Category("Integration")]
public async Task GetPaymentLink_WithValidRequest_ReturnsSuccessfulResult()
{
    // Arrange
    var paymentService = CreateConfiguredPaymentService(); // Helper method
    var paymentRequest = CreateSamplePaymentRequest();      // Helper method
    
    // Act
    var result = await paymentService.GetPaymentLink(paymentRequest);

    // Assert
    Assert.Multiple(() =>
    {
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.PaymentUrl, Is.Not.Null.And.Not.Empty);
        Assert.That(result.PaymentReference, Is.Not.Null.And.Not.Empty);
    });
}
```

**Plugin Test with Dataverse:**
```csharp
[Test]
public void PostCreate_WhenContactCreated_ShouldUpdateCalculatedFields()
{
    // Arrange
    var contactId = Guid.NewGuid();
    var targetContact = new DataverseModels.Contact
    {
        Id = contactId,
        FirstName = "John",
        LastName = "Doe",
        si_Savings = new Money(1000m),
        si_Liabilities = new Money(500m)
    };
    
    context.Initialize(new List<Entity> { targetContact });
    
    var pluginContext = context.GetDefaultPluginContext();
    pluginContext.InputParameters["Target"] = targetContact;
    pluginContext.MessageName = "Create";
    pluginContext.Stage = (int)ProcessingStage.PostOperation;

    // Act
    context.ExecutePluginWith<ContactPostCreate>(pluginContext);

    // Assert
    var updatedContact = context.CreateQuery<DataverseModels.Contact>()
        .FirstOrDefault(c => c.Id == contactId);
    
    Assert.That(updatedContact, Is.Not.Null);
    Assert.That(updatedContact.si_NetWorth?.Value, Is.EqualTo(500m)); // 1000 - 500
}
```

#### Common AAA Pattern Variations

**Given-When-Then (BDD Style):**
```csharp
[Test]
public void PaymentProcessor_ShouldProcessPayment_WhenValidDataProvided()
{
    // Given (Arrange)
    var processor = new PaymentProcessor();
    var validPaymentData = new PaymentData { Amount = 100m };

    // When (Act)
    var result = processor.Process(validPaymentData);

    // Then (Assert)
    Assert.That(result.IsSuccessful, Is.True);
}
```

**Setup-Exercise-Verify (for mocking frameworks):**
```csharp
[Test]
public void PaymentService_ShouldCallRepository_WhenProcessingPayment()
{
    // Setup (Arrange)
    var fakeRepository = A.Fake<IPaymentRepository>();
    var service = new PaymentService(fakeRepository);
    var payment = new Payment();

    // Exercise (Act)
    service.ProcessPayment(payment);

    // Verify (Assert)
    A.CallTo(() => fakeRepository.Save(payment))
        .MustHaveHappenedOnceExactly();
}
```

#### Advanced AAA Patterns

**Multiple Assert Sections for Complex Scenarios:**
```csharp
[Test]
public void ProcessPayment_ShouldUpdateMultipleEntities()
{
    // Arrange
    var payment = CreatePayment();
    var processor = new PaymentProcessor();

    // Act
    var result = processor.ProcessPayment(payment);

    // Assert - Verify return value
    Assert.That(result.IsSuccess, Is.True);
    
    // Assert - Verify database state
    var savedPayment = GetPaymentFromDatabase(payment.Id);
    Assert.That(savedPayment.Status, Is.EqualTo(PaymentStatus.Processed));
    
    // Assert - Verify side effects
    var auditLog = GetAuditLog(payment.Id);
    Assert.That(auditLog, Is.Not.Null);
    Assert.That(auditLog.Action, Is.EqualTo("PaymentProcessed"));
}
```

### Test Project Structure and Organization
1. **Test Project Naming Convention**: `<ProjectName>.Tests`
2. **Test Class Organization**: Mirror the source project structure (Services, Helpers, Mappers, etc.)
3. **Test Class Naming**: `<ClassUnderTest>Tests` (e.g., `PaymentServiceTests`, `WorldpayXmlBuilderTests`)
4. **Test Method Naming**: Use descriptive names following pattern `MethodName_Scenario_ExpectedResult`

### Framework-Specific Configurations

#### Domain Project Tests (.NET 8.0)
**Required NuGet Packages:**
```xml
<PackageReference Include="FakeItEasy" Version="8.3.0" />
<PackageReference Include="NUnit" Version="4.3.2" />
<PackageReference Include="NUnit3TestAdapter" Version="5.0.0" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.13.0" />
<PackageReference Include="NUnit.Analyzers" Version="4.6.0" />
<PackageReference Include="coverlet.collector" Version="6.0.4" />
```

**Additional packages for HTTP/Configuration testing:**
```xml
<PackageReference Include="Microsoft.AspNetCore.Http" Version="2.2.2" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="9.0.3" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="9.0.3" />
```

#### Dataverse Infrastructure Tests (.NET 8.0)
**Required NuGet Packages:**

FakeXrmEasy.v9 is based on FakeItEasy library and contains it as dependency.

```xml
<PackageReference Include="FakeXrmEasy.v9" Version="3.6.0" />
<PackageReference Include="NUnit" Version="4.3.2" />
<PackageReference Include="NUnit3TestAdapter" Version="5.0.0" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.13.0" />
```

#### Plugin Tests (.NET Framework 4.8)
**Required NuGet Packages:**
```xml
<PackageReference Include="FakeXrmEasy.v9" Version="2.5.0" />
<PackageReference Include="NUnit" Version="4.3.2" />
<PackageReference Include="NUnit3TestAdapter" Version="4.6.0" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
<PackageReference Include="Microsoft.CrmSdk.CoreAssemblies" Version="9.0.2.*" />
```

### Test Categories and Attributes

#### Test Categories
- Use `[Category("Integration")]` for integration tests
- Use `[TestFixture]` for test classes
- Use `[OneTimeSetUp]` for expensive setup operations
- Use `[SetUp]` for per-test initialization
- Use `[Test]` for individual test methods

#### Test Lifecycle Management
```csharp
[TestFixture]
public class ExampleServiceTests
{
    private Service serviceUnderTest;
    private IDependency fakeDependency;

    [OneTimeSetUp] // For expensive operations done once per test class
    public void OneTimeSetUp()
    {
        // Configuration loading, DI container setup, etc.
    }

    [SetUp] // For per-test setup
    public void Setup()
    {
        // Create fakes and initialize service under test
        fakeDependency = A.Fake<IDependency>();
        serviceUnderTest = new Service(fakeDependency);
    }
}
```

### Mocking and Faking Patterns

#### FakeItEasy Usage for Domain Tests
```csharp
// Creating fakes
var fakeRepository = A.Fake<IRepository>();
var fakeLogger = A.Fake<ILogger<Service>>();

// Configuring fake behavior
A.CallTo(() => fakeRepository.GetAsync(A<Guid>._))
    .Returns(Task.FromResult(expectedEntity));

// Verifying calls
A.CallTo(() => fakeRepository.SaveAsync(A<Entity>._))
    .MustHaveHappenedOnceExactly();
```

#### Custom Test Adapters
For logging, create custom test adapters:
```csharp
public class TestLoggingAdapter<T> : ILogger<T>
{
    public void Log(LogLevel level, string message, Exception? ex = null)
    {
        Console.WriteLine($"{level}:{message}");
    }
}
```

#### Faking SI.Common.Domain.Logging.ILogger
When testing services that use the SI.Common.Domain.Logging.ILogger interface, you can fake the logger and capture logged messages for verification:

```csharp
[Test]
public async Task SetupPayment_WhenPaymentFails_ShouldLogError()
{
    // Arrange
    var fakeLogger = A.Fake<ILogger<PaymentService>>();
    var loggedMessages = new List<string>();
    
    // Configure fake to capture logged messages
    A.CallTo(() => fakeLogger.Log(LogLevel.Information, A<string>.Ignored, A<Exception>.Ignored))
        .Invokes((LogLevel logLevel, string message, Exception ex) => loggedMessages.Add(message));
    
    A.CallTo(() => fakeLogger.Log(LogLevel.Error, A<string>.Ignored, A<Exception>.Ignored))
        .Invokes((LogLevel logLevel, string message, Exception ex) => loggedMessages.Add(message));
    
    var paymentService = new PaymentService(fakeLogger, /* other dependencies */);
    var paymentRequest = CreateInvalidPaymentRequest(); // Helper method

    // Act
    var result = await paymentService.SetupPayment(paymentRequest);

    // Assert
    Assert.Multiple(() =>
    {
        Assert.That(loggedMessages.Any(x => x.Contains("Payment link generation failed")), Is.True);
        Assert.That(result, Is.EqualTo(expectedResult));
    });
}
```

**Alternative approach - Verify specific log calls:**
```csharp
[Test]
public async Task ProcessPayment_WhenSuccessful_ShouldLogInformation()
{
    // Arrange
    var fakeLogger = A.Fake<ILogger<PaymentService>>();
    var paymentService = new PaymentService(fakeLogger, /* other dependencies */);
    var paymentRequest = CreateValidPaymentRequest();

    // Act
    await paymentService.ProcessPayment(paymentRequest);

    // Assert - Verify specific log calls were made
    A.CallTo(() => fakeLogger.Log(
        LogLevel.Information, 
        A<string>.That.Contains("Processing payment for amount"), 
        A<Exception>.Ignored))
        .MustHaveHappenedOnceExactly();
}
```

**Capturing logs with different log levels:**
```csharp
[Test]
public async Task PaymentService_ShouldLogAppropriateMessages()
{
    // Arrange
    var fakeLogger = A.Fake<ILogger<PaymentService>>();
    var infoMessages = new List<string>();
    var errorMessages = new List<string>();
    
    // Capture Information logs
    A.CallTo(() => fakeLogger.Log(LogLevel.Information, A<string>.Ignored, A<Exception>.Ignored))
        .Invokes((LogLevel level, string message, Exception ex) => infoMessages.Add(message));
    
    // Capture Error logs
    A.CallTo(() => fakeLogger.Log(LogLevel.Error, A<string>.Ignored, A<Exception>.Ignored))
        .Invokes((LogLevel level, string message, Exception ex) => errorMessages.Add(message));
    
    var paymentService = new PaymentService(fakeLogger, /* other dependencies */);

    // Act
    await paymentService.ProcessPayment(paymentRequest);

    // Assert
    Assert.Multiple(() =>
    {
        Assert.That(infoMessages, Has.Count.GreaterThan(0));
        Assert.That(infoMessages.Any(msg => msg.Contains("Payment processing started")), Is.True);
        Assert.That(errorMessages, Is.Empty); // No errors should be logged for successful payment
    });
}
```

### Dataverse Testing Patterns

#### FakeXrmEasy Setup (.NET 8.0 - Infrastructure)

IDataverseServiceFactory needs to return FakeXrmEasy IOrganizationServiceAsync2 fakeXrmService as Service Instance in order for FakeXrmEasy context to return objects in queries.

```csharp
using Microsoft.PowerPlatform.Dataverse.Client;
using FakeXrmEasy.Abstractions.Enums;

[SetUp]
public void Setup()
{
    // Initialize FakeXrmEasy context for .NET 8.0 Infrastructure testing
    context = MiddlewareBuilder
        .New()
        .AddCrud()
        .UseCrud()
        .SetLicense(FakeXrmEasyLicense.Commercial) // Use Commercial for .NET 8.0
        .Build();

    // Get the FakeXrmEasy organization service for actual data operations
    var fakeXrmService = context.GetAsyncOrganizationService2();

    // Setup fakes
    fakeServiceFactory = A.Fake<IDataverseServiceFactory>();
    fakeUnitOfWork = A.Fake<IUnitOfWork>();
    fakeLogger = A.Fake<ILogger<DocumentLocationRepository>>();

    A.CallTo(() => fakeServiceFactory.GetNextServiceInstance()).Returns(fakeXrmService);

    // Initialize repository under test
    repository = new DocumentLocationRepository(fakeServiceFactory, fakeUnitOfWork, fakeLogger);
}

```

#### FakeXrmEasy Setup (.NET 4.8 - Plugins)
```csharp
[SetUp]
public void Setup()
{
    context = MiddlewareBuilder
        .New()
        .AddCrud()
        .UseCrud()
        .SetLicense(FakeXrmEasyLicense.NonCommercial) // Use NonCommercial for .NET 4.8
        .Build();

    service = context.GetOrganizationService(); // Use sync service for .NET 4.8
}
```

#### Entity Creation and Context Initialization
```csharp
// Creating test entities
var testEntity = new EntityName
{
    Id = Guid.NewGuid(),
    AttributeName = "Test Value"
};

// Adding entities to context
context.Initialize(new List<Entity> { testEntity });

// Or add individual entities
context.AddEntity(testEntity);
```

#### Plugin Execution Context Setup
```csharp
// Setup plugin execution context
var pluginContext = context.GetDefaultPluginContext();
pluginContext.InputParameters["Target"] = targetEntity;
pluginContext.MessageName = "Create";
pluginContext.Stage = (int)ProcessingStage.PostOperation;
pluginContext.Mode = (int)ProcessingMode.Synchronous;

// Execute plugin
context.ExecutePluginWith<YourPlugin>(pluginContext);
```

### Assertion Patterns

#### NUnit Constraint Model (Preferred)
**Always use the NUnit constraint model for assertions** instead of the classic Assert methods. The constraint model provides:
- **Better readability**: More natural language-like assertions
- **Improved error messages**: Clearer failure descriptions
- **Enhanced functionality**: Rich constraint combinations and custom constraints
- **Future compatibility**: Microsoft and NUnit team recommend constraint model

**Preferred Constraint Model:**
```csharp
// ✅ PREFERRED - Use constraint model
Assert.That(result, Is.Not.Null);
Assert.That(result.Value, Is.EqualTo(expectedValue));
Assert.That(collection, Has.Count.EqualTo(3));
Assert.That(text, Does.StartWith("Hello"));
Assert.That(number, Is.GreaterThan(0).And.LessThan(100));
Assert.That(list, Contains.Item(expectedItem));
Assert.That(dateTime, Is.EqualTo(expectedDate).Within(TimeSpan.FromSeconds(1)));
Assert.That(result.IsSuccess, Is.True);
Assert.That(exception, Is.TypeOf<ArgumentNullException>());
```

**Avoid Classic Assert Methods:**
```csharp
// ❌ AVOID - Classic assert methods
Assert.IsNotNull(result);           // Use Assert.That(result, Is.Not.Null)
Assert.AreEqual(expected, actual);  // Use Assert.That(actual, Is.EqualTo(expected))
Assert.IsTrue(condition);           // Use Assert.That(condition, Is.True)
Assert.Greater(actual, expected);   // Use Assert.That(actual, Is.GreaterThan(expected))
Assert.Contains(item, collection);  // Use Assert.That(collection, Contains.Item(item))
```

#### NUnit Constraint Model
Always use the constraint model for assertions:
```csharp
// Single assertions
Assert.That(result, Is.Not.Null);
Assert.That(result.Value, Is.EqualTo(expectedValue));
Assert.That(collection, Has.Count.EqualTo(3));

// Multiple assertions (preferred for related checks)
Assert.Multiple(() =>
{
    Assert.That(entity.Id, Is.Not.EqualTo(Guid.Empty));
    Assert.That(entity.Name, Is.EqualTo("Expected Name"));
    Assert.That(entity.Status, Is.EqualTo(ExpectedStatus.Active));
});
```

#### Complex Object Assertions
```csharp
// For domain models
Assert.Multiple(() =>
{
    Assert.That(result.IsSuccess, Is.True, $"Failed with error: {result.ErrorMessage}");
    Assert.That(result.PaymentUrl, Is.Not.Null.And.Not.Empty);
    Assert.That(result.PaymentReference, Is.Not.Null.And.Not.Empty);
});

// For Dataverse entities
var updatedEntity = context.CreateQuery<EntityName>()
    .FirstOrDefault(e => e.Id == entityId)
    .ToEntity<EntityName>();

Assert.That(updatedEntity, Is.Not.Null);
Assert.Multiple(() =>
{
    Assert.That(updatedEntity.Field1, Is.EqualTo(expectedValue1));
    Assert.That(updatedEntity.Field2, Is.EqualTo(expectedValue2));
});
```

### Integration Testing Patterns

#### Configuration Management
```csharp
[OneTimeSetUp]
public void OneTimeSetUp()
{
    var configBuilder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

    configuration = configBuilder.Build();

    // Bind configuration settings
    var settings = new ServiceSettings();
    configuration.GetSection("ServiceName").Bind(settings);
}
```

#### Integration Test Safety
```csharp
[Test]
[Category("Integration")]
public async Task IntegrationTest_Method()
{
    // Skip test if configuration not available
    if (string.IsNullOrEmpty(settings?.RequiredProperty))
    {
        Assert.Ignore("Configuration not provided. Skipping integration test.");
    }
    
    // Test implementation
}
```

#### HttpClientFactory for Integration Tests
```csharp
// Setup DI for HttpClientFactory
var services = new ServiceCollection();
services.AddHttpClient();
var serviceProvider = services.BuildServiceProvider();
var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
```

### Test Data Management

#### Model Builder Patterns
```csharp
private PaymentRequestDto CreateSamplePaymentRequest()
{
    return new PaymentRequestDto
    {
        Amount = 100.00m,
        Description = "Test Payment",
        EmailAddress = "test@example.com",
        // ... other properties
    };
}
```

#### SQL Repository Testing Patterns
```csharp
[SetUp]
public void Setup()
{
    // Setup fake SQL objects using interfaces
    fakeConnection = A.Fake<ISqlConnection>();
    fakeCommand = A.Fake<ISqlCommand>();
    fakeDataReader = A.Fake<ISqlDataReader>();
    fakeParameters = A.Fake<ISqlParameterCollection>();
    
    // Configure fake behaviors
    A.CallTo(() => sqlConnectionFactory.CreateConnectionAsync())
        .Returns(Task.FromResult(fakeConnection));
    A.CallTo(() => fakeConnection.CreateCommand()).Returns(fakeCommand);
}
```

### HTTP Request Testing
```csharp
[Test]
public void ProcessPayment_WithValidXmlRequest_ReturnsSuccess()
{
    // Arrange
    var context = new DefaultHttpContext();
    var request = context.Request;
    
    request.Body = new MemoryStream(Encoding.UTF8.GetBytes(xmlContent));
    
    // Act & Assert
}
```

### Configuration Files in Tests
Include configuration files for integration tests:
```xml
<ItemGroup>
    <None Update="appsettings.Development.json">
        <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </None>
    <None Update="appsettings.json">
        <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </None>
</ItemGroup>
```

### Global Using Statements
For .NET 8.0 test projects, include:
```csharp
global using NUnit.Framework;
```

### Test Output and Debugging
```csharp
// For integration test output
await TestContext.Out.WriteLineAsync($"Payment URL: {result.PaymentUrl}");
await TestContext.Out.WriteLineAsync($"Payment Reference: {result.PaymentReference}");
```

## Unit Testing Summary

### Domain Projects (.NET 8.0)
- **Frameworks**: NUnit + FakeItEasy
- **Focus**: Business logic testing, service layer testing
- **Patterns**: Arrange-Act-Assert, dependency injection with fakes
- **Special**: Custom logging adapters, HTTP context testing

### Infrastructure Projects (.NET 8.0)
- **Frameworks**: NUnit + FakeXrmEasy (Commercial license)
- **Focus**: Repository testing, Dataverse integration
- **Patterns**: Entity creation/retrieval testing, async service usage
- **Special**: Dataverse entity mapping, CRUD operations

### Plugin Projects (.NET 4.8)
- **Frameworks**: NUnit + FakeXrmEasy (NonCommercial license)
- **Focus**: Plugin execution, Dataverse context manipulation
- **Patterns**: Plugin context setup, entity lifecycle testing
- **Special**: Environment variable configuration, early-bound entities

## Key Testing Principles
1. **Isolation**: Each test should be independent and not rely on others
2. **Repeatability**: Tests should produce consistent results
3. **Clear Intent**: Test names and structure should clearly indicate what's being tested
4. **Comprehensive Coverage**: Test happy paths, edge cases, and error conditions
5. **Maintainability**: Use helper methods and builders for complex test data
6. **Performance**: Use `[OneTimeSetUp]` for expensive operations

## Example: Complete Dataverse Plugin Test

```csharp
public class ContactPostCreateTests
{
    private IXrmFakedContext context;
    private IOrganizationService service;

    [SetUp]
    public void Setup()
    {
        // Initialize FakeXrmEasy context with required middleware
        context = MiddlewareBuilder
            .New()
            .AddCrud()
            .UseCrud()
            .SetLicense(FakeXrmEasyLicense.Commercial)
            .Build();

        service = context.GetOrganizationService();

        // Enable proxy types for early-bound entities
        var assembly = typeof(DataverseModels.ClientNameContext).Assembly;
        context.EnableProxyTypes(assembly);
    }

    [Test]
    public void PostCreate_WhenContactCreated_ShouldSetNetWorth()
    {
        // Arrange
        var contactId = Guid.NewGuid();
        var targetContact = new DataverseModels.Contact
        {
            Id = contactId,
            FirstName = "John",
            LastName = "Doe",
            si_Savings = new Money(1000m),
            si_Liabilities = new Money(500m)
        };

        // Add entities to the fake Dataverse context
        context.Initialize(new List<Entity> { targetContact });

        // Setup plugin execution context
        var pluginContext = context.GetDefaultPluginContext();
        pluginContext.InputParameters["Target"] = targetContact;
        pluginContext.MessageName = "Create";
        pluginContext.Stage = (int)ProcessingStage.PostOperation;
        pluginContext.Mode = (int)ProcessingMode.Synchronous;

        // If environment variables are needed
        var envVariable = new EnvironmentVariableDefinition
        {
            Id = Guid.NewGuid(),
            SchemaName = "si_ApiKey",
            DefaultValue = "test-api-key"
        };
        context.AddEntity(envVariable);

        // Act
        context.ExecutePluginWith<PostCreate>(pluginContext);

        // Assert
        var updatedContact = context.CreateQuery(DataverseModels.Contact.EntityLogicalName)
            .FirstOrDefault(e => e.Id == contactId)
            .ToEntity<DataverseModels.Contact>();

        Assert.That(updatedContact, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(updatedContact.si_Savings.Value, Is.EqualTo(1000m));
            Assert.That(updatedContact.si_Liabilities.Value, Is.EqualTo(500m));
            Assert.That(updatedContact.si_UpdatedByProcess, Is.Not.Null);
        });
    }
}
```
