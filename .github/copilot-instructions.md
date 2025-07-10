# GitHub Copilot Instructions

These instructions define how GitHub Copilot should assist with this project. The goal is to ensure consistent, high-quality code generation aligned with our conventions, stack, and best practices.

## 🧠 Context

- **Project Type**: Web Application
- **Language**: C#
- **Framework / Libraries**: .NET 9 / ASP.NET Core / Entity Framework Core
- **Testing Framework**: xUnit, NSubstitute, AwesomeAssertions, TestContainers
- **Architecture**: Modular Monolith; Clean Architecture; Vertical Slice Architecture

## .NET/C# Best Practices

Your task is to ensure .NET/C# code in ${selection} meets the best practices specific to this solution/project. This includes:

### Documentation & Structure

- Create comprehensive XML documentation comments for all public classes, interfaces, methods, and properties
- Include parameter descriptions and return value descriptions in XML comments
- Follow the established namespace structure: {Core|Console|App|Service}.{Feature}

### Design Patterns & Architecture

- Use primary constructor syntax for dependency injection (e.g., `public class MyClass(IDependency dependency)`)
- Implement the Command Handler pattern with generic base classes (e.g., `CommandHandler<TOptions>`)
- Use interface segregation with clear naming conventions (prefix interfaces with 'I')
- Follow the Factory pattern and/or Builder pattern for complex object creation.

### Dependency Injection & Services

- Use constructor dependency injection with null checks via ArgumentNullException
- Register services with appropriate lifetimes (Singleton, Scoped, Transient)
- Use Microsoft.Extensions.DependencyInjection patterns
- Implement service interfaces for testability

### Resource Management & Localization

- Use ResourceManager for localized messages and error strings
- Separate LogMessages and ErrorMessages resource files
- Access resources via `_resourceManager.GetString("MessageKey")`

### Async/Await Patterns

- Use async/await for all I/O operations and long-running tasks
- Return Task or Task<T> from async methods
- Use ConfigureAwait(false) where appropriate
- Handle async exceptions properly

### Testing Standards

- Use xUnit framework with FluentAssertions for assertions
- Follow AAA pattern (Arrange, Act, Assert)
- Use NSubstitute for mocking dependencies
- Test both success and failure scenarios
- Include null parameter validation tests

### Configuration & Settings

- Use strongly-typed configuration classes with data annotations
- Implement validation attributes (Required, NotEmptyOrWhitespace)
- Use IConfiguration binding for settings
- Support appsettings.json configuration files

### Semantic Kernel & AI Integration

- Use Microsoft.SemanticKernel for AI operations
- Implement proper kernel configuration and service registration
- Handle AI model settings (ChatCompletion, Embedding, etc.)
- Use structured output patterns for reliable AI responses

### Error Handling & Logging

- Use structured logging with Microsoft.Extensions.Logging
- Include scoped logging with meaningful context
- Throw specific exceptions with descriptive messages
- Use try-catch blocks for expected failure scenarios

### Performance & Security

- Use C# 13+ features and .NET 10 optimizations where applicable
- Implement proper input validation and sanitization
- Use parameterized queries for database operations
- Follow secure coding practices for AI/ML operations

### Code Quality

- Ensure SOLID principles compliance
- Avoid code duplication through base classes and utilities
- Use meaningful names that reflect domain concepts
- Keep methods focused and cohesive
- Implement proper disposal patterns for resources

## C# Async Programming Best Practices

Your goal is to help me follow best practices for asynchronous programming in C#.

### Naming Conventions

- Use the 'Async' suffix for all async methods
- Match method names with their synchronous counterparts when applicable (e.g., `GetDataAsync()` for `GetData()`)

### Return Types

- Return `Task<T>` when the method returns a value
- Return `Task` when the method doesn't return a value
- Consider `ValueTask<T>` for high-performance scenarios to reduce allocations
- Avoid returning `void` for async methods except for event handlers

### Exception Handling

- Use try/catch blocks around await expressions
- Avoid swallowing exceptions in async methods
- Use `ConfigureAwait(false)` when appropriate to prevent deadlocks in library code
- Propagate exceptions with `Task.FromException()` instead of throwing in async Task returning methods

### Performance

- Use `Task.WhenAll()` for parallel execution of multiple tasks
- Use `Task.WhenAny()` for implementing timeouts or taking the first completed task
- Avoid unnecessary async/await when simply passing through task results
- Consider cancellation tokens for long-running operations

### Common Pitfalls

- Never use `.Wait()`, `.Result`, or `.GetAwaiter().GetResult()` in async code
- Avoid mixing blocking and async code
- Don't create async void methods (except for event handlers)
- Always await Task-returning methods

### Implementation Patterns

- Implement the async command pattern for long-running operations
- Use async streams (IAsyncEnumerable<T>) for processing sequences asynchronously
- Consider the task-based asynchronous pattern (TAP) for public APIs

When reviewing my C# code, identify these issues and suggest improvements that follow these best practices.

## C# Documentation Best Practices

- Public members should be documented with XML comments.
- It is encouraged to document internal members as well, especially if they are complex or not self-explanatory.
- Use `<summary>` for method descriptions. This should be a brief overview of what the method does.
- Use `<param>` for method parameters.
- Use `<returns>` for method return values.
- Use `<remarks>` for additional information, which can include implementation details, usage notes, or any other relevant context.
- Use `<example>` for usage examples on how to use the member.
- Use `<exception>` to document exceptions thrown by methods.
- Use `<see>` and `<seealso>` for references to other types or members.
- Use `<inheritdoc/>` to inherit documentation from base classes or interfaces.
  - Unless there is major behavior change, in which case you should document the differences.
- Use `<typeparam>` for type parameters in generic types or methods.
- Use `<typeparamref>` to reference type parameters in documentation.
- Use `<c>` for inline code snippets.
- Use `<code>` for code blocks.

## XUnit Best Practices

Your goal is to help me write effective unit tests with XUnit, covering both standard and data-driven testing approaches.

### Project Setup

- Use a separate test project with naming convention `[ProjectName].Tests`
- Reference Microsoft.NET.Test.Sdk, xunit, and xunit.runner.visualstudio packages
- Create test classes that match the classes being tested (e.g., `CalculatorTests` for `Calculator`)
- Use .NET SDK test commands: `dotnet test` for running tests

### Test Structure

- No test class attributes required (unlike MSTest/NUnit)
- Use fact-based tests with `[Fact]` attribute for simple tests
- Follow the Arrange-Act-Assert (AAA) pattern
- Name tests using the pattern `MethodName_Scenario_ExpectedBehavior`
- Use constructor for setup and `IDisposable.Dispose()` for teardown
- Use `IClassFixture<T>` for shared context between tests in a class
- Use `ICollectionFixture<T>` for shared context between multiple test classes

### Standard Tests

- Keep tests focused on a single behavior
- Avoid testing multiple behaviors in one test method
- Use clear assertions that express intent
- Include only the assertions needed to verify the test case
- Make tests independent and idempotent (can run in any order)
- Avoid test interdependencies

### Data-Driven Tests

- Use `[Theory]` combined with data source attributes
- Use `[InlineData]` for inline test data
- Use `[MemberData]` for method-based test data
- Use `[ClassData]` for class-based test data
- Create custom data attributes by implementing `DataAttribute`
- Use meaningful parameter names in data-driven tests

### Assertions

- Use AwesomeAssertions for fluent assertions in tests
- Avoid the use of `Assert` directly; prefer using the fluent API

### Mocking and Isolation

- Use NSubstitute for mocking dependencies in tests
- Mock dependencies to isolate units under test
- Use interfaces to facilitate mocking
- Use Microsoft.Extensions.Diagnostics.Testing and the `FakeLogger<T>` for testing logging behavior.
- Consider using a DI container for complex test setups

### Database and Integration Tests

- Use TestContainers for integration tests that require external services (e.g., databases)
- Avoid the use of in-memory databases for integration tests; prefer TestContainers for realistic scenarios
- Use `[CollectionDefinition]` and `[Collection]` attributes to group tests and disable parallelization when using TestContainers

### Test Organization

- Group tests by feature or component
- Use `[Trait("Category", "CategoryName")]` for categorization
- Use collection fixtures to group tests with shared dependencies
- Consider output helpers (`ITestOutputHelper`) for test diagnostics
- Skip tests conditionally with `Skip = "reason"` in fact/theory attributes

## Entity Framework Core Best Practices

Your goal is to help me follow best practices when working with Entity Framework Core.

### Data Context Design

- Keep DbContext classes focused and cohesive
- Use constructor injection for configuration options
- Override OnModelCreating for fluent API configuration
- Separate entity configurations using IEntityTypeConfiguration
- Consider using DbContextFactory pattern for console apps or tests

### Entity Design

- Use meaningful primary keys (consider natural vs surrogate keys)
- Implement proper relationships (one-to-one, one-to-many, many-to-many)
- Use data annotations or fluent API for constraints and validations
- Implement appropriate navigational properties
- Consider using owned entity types for value objects

### Performance

- Use AsNoTracking() for read-only queries
- Implement pagination for large result sets with Skip() and Take()
- Use Include() to eager load related entities when needed
- Consider projection (Select) to retrieve only required fields
- Use compiled queries for frequently executed queries
- Avoid N+1 query problems by properly including related data

### Migrations

- Do not use EF Core Migrations.
- Use DbUp for schema upgrades, packaging as a separate console app.
- Name SQL scripts descriptively, organized so that they can be run in order.
- Add data seeding through SQL scripts during schema upgrade where appropriate.

## Querying

- Use IQueryable judiciously and understand when queries execute
- Prefer strongly-typed LINQ queries over raw SQL
- Use appropriate query operators (Where, OrderBy, GroupBy)
- Consider database functions for complex operations
- Implement specifications pattern for reusable queries

## Change Tracking & Saving

- Use appropriate change tracking strategies
- Batch your SaveChanges() calls
- Implement concurrency control for multi-user scenarios
- Consider using transactions for multiple operations
- Use appropriate DbContext lifetimes (scoped for web apps)

## Security

- Avoid SQL injection by using parameterized queries
- Implement appropriate data access permissions
- Be careful with raw SQL queries
- Consider data encryption for sensitive information
- Use migrations to manage database user permissions

## Testing

- Use in-memory database provider for unit tests
- Create separate testing contexts with SQLite for integration tests
- Mock DbContext and DbSet for pure unit tests
- Test migrations in isolated environments
- Consider snapshot testing for model changes

When reviewing my EF Core code, identify issues and suggest improvements that follow these best practices.

## C# Instructions

- Always use the latest version C#, currently C# 13 features.
- Write clear and concise comments for each function.

## General Instructions

- Make only high confidence suggestions when reviewing code changes.
- Write code with good maintainability practices, including comments on why certain design decisions were made.
- Handle edge cases and write clear exception handling.
- For libraries or external dependencies, mention their usage and purpose in comments.

## Naming Conventions

- Follow PascalCase for component names, method names, and public members.
- Use camelCase for private fields and local variables.
- Prefix interface names with "I" (e.g., IUserService).

## Formatting

- Apply code-formatting style defined in `.editorconfig`.
- Prefer file-scoped namespace declarations and single-line using directives.
- Insert a newline before the opening curly brace of any code block (e.g., after `if`, `for`, `while`, `foreach`, `using`, `try`, etc.).
- Ensure that the final return statement of a method is on its own line.
- Use pattern matching and switch expressions wherever possible.
- Use `nameof` instead of string literals when referring to member names.
- Ensure that XML doc comments are created for any public APIs. When applicable, include `<example>` and `<code>` documentation in the comments.

## Project Setup and Structure

- Guide users through creating a new .NET project with the appropriate templates.
- Explain the purpose of each generated file and folder to build understanding of the project structure.
- Demonstrate how to organize code using feature folders or domain-driven design principles.
- Show proper separation of concerns with models, services, and data access layers.
- Explain the Program.cs and configuration system in ASP.NET Core 9 including environment-specific settings.

## Nullable Reference Types

- Declare variables non-nullable, and check for `null` at entry points.
- Always use `is null` or `is not null` instead of `== null` or `!= null`.
- Trust the C# null annotations and don't add null checks when the type system says a value cannot be null.
- Use `??` for null-coalescing and `??=` for null-coalescing assignment.
- Use `[NotNullWhen]` to indicate that a method will return a non-null value in an out var under certain conditions.

## Data Access Patterns

- Guide the implementation of a data access layer using Entity Framework Core.
- Explain different options (SQL Server, SQLite, In-Memory) for development and production.
- Demonstrate repository pattern implementation and when it's beneficial.
- Show how to implement database migrations and data seeding.
- Explain efficient query patterns to avoid common performance issues.

## Authentication and Authorization

- Guide users through implementing authentication using JWT Bearer tokens.
- Explain OAuth 2.0 and OpenID Connect concepts as they relate to ASP.NET Core.
- Show how to implement role-based and policy-based authorization.
- Demonstrate integration with Microsoft Entra ID (formerly Azure AD).
- Explain how to secure both controller-based and Minimal APIs consistently.

## Validation and Error Handling

- Guide the implementation of model validation using data annotations and FluentValidation.
- Explain the validation pipeline and how to customize validation responses.
- Demonstrate a global exception handling strategy using middleware.
- Show how to create consistent error responses across the API.
- Explain problem details (RFC 7807) implementation for standardized error responses.

## API Versioning and Documentation

- Guide users through implementing and explaining API versioning strategies.
- Demonstrate Swagger/OpenAPI implementation with proper documentation.
- Show how to document endpoints, parameters, responses, and authentication.
- Explain versioning in both controller-based and Minimal APIs.
- Guide users on creating meaningful API documentation that helps consumers.

## Logging and Monitoring

- Guide the implementation of structured logging using Serilog or other providers.
- Explain the logging levels and when to use each.
- Demonstrate integration with Application Insights for telemetry collection.
- Show how to implement custom telemetry and correlation IDs for request tracking.
- Explain how to monitor API performance, errors, and usage patterns.

## Testing

- Always include test cases for critical paths of the application.
- Guide users through creating unit tests.
- Do not emit "Act", "Arrange" or "Assert" comments.
- Copy existing style in nearby files for test method names and capitalization.
- Explain integration testing approaches for API endpoints.
- Demonstrate how to mock dependencies for effective testing.
- Show how to test authentication and authorization logic.
- Explain test-driven development principles as applied to API development.

## Performance Optimization

- Guide users on implementing caching strategies (in-memory, distributed, response caching).
- Explain asynchronous programming patterns and why they matter for API performance.
- Demonstrate pagination, filtering, and sorting for large data sets.
- Show how to implement compression and other performance optimizations.
- Explain how to measure and benchmark API performance.

## Deployment and DevOps

- Guide users through containerizing their API using .NET's built-in container support (`dotnet publish --os linux --arch x64 -p:PublishProfile=DefaultContainer`).
- Explain the differences between manual Dockerfile creation and .NET's container publishing features.
- Explain CI/CD pipelines for NET applications.
- Demonstrate deployment to Azure App Service, Azure Container Apps, or other hosting options.
- Show how to implement health checks and readiness probes.
- Explain environment-specific configurations for different deployment stages.
- Avoid fat controllers and God classes.

### Projects, Packages, and Dependencies

- Create a solution using `dotnet new sln`.
- Create projects using `dotnet new` with appropriate templates:
  - `dotnet new web`
  - `dotnet new classlib`
  - `dotnet new xunit`
  - `dotnet new aspire-apphost`
  - `dotnet new aspire-servicedefaults`
- Use `dotnet add package` to add NuGet packages.  Use the latest stable versions.
- Use `dotnet add reference` to link projects within the solution.
- Use `dotnet sln add` to add projects to the solution.
- Avoid editing the `.sln` and `.csproj` files manually unless necessary.

## Database Guidelines
- Use PostgreSQL postgres:17-alpine (using TestContainers for integration tests)
- Use CITEXT (case-insensitive) and TEXT (normal text) instead of VARCHAR
- Use UUID-OSSP for UUID generation
- Use DbUp based schema upgrades, packaging as a separate console app (for better integration with Aspire)
- Always use a separate (non-"public") schema for storing application tables

## UI Implementations

- Web: MVC, Razor Views, organized in areas.
- Client Frameworks: Bootstrap 5, jQuery, bootstrap-icons
- Use Libman to organize libraries

## Approved libraries

- AngleSharp
- AutoMapper
- AutoFixture
- AwesomeAssertions
- DbUp
- FastEndpoints
- FluentValidation
- Hangfire
- HotChocolate
- MediatR
- Npgsql
- NSubstitute
- NSwag
- OpenIddict
- Polly
- Quartz.NET
- RabbitMQ
- Refit
- Scalar
- StackExchange.Redis
- TestContainers
- xUnit

## Libraries you MUST NOT USE (with Alternatives)

- FluentAssertions (use AwesomeAssertions)
- IdentityServer (use OpenIddict)
- Moq (use NSubstitute)
- NUnit or MSTest (use xUnit)
- Serilog or NLog (use built-in logging for ASP.NET Core or .NET Generic Host)

## 🔁 Iteration & Review
- Copilot output should be reviewed and modified before committing.
- If code isn’t following these instructions, regenerate with more context or split the task.
- Use /// XML documentation comments to clarify intent for Copilot and future devs.
- Use Visual Studio code inspections to catch violations early.

## 📚 References
- [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/?view=aspnetcore-8.0)
- [Entity Framework Core Docs](https://learn.microsoft.com/en-us/ef/core/)
- [xUnit Documentation](https://xunit.net/)