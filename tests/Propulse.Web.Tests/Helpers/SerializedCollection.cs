namespace Propulse.Web.Tests.Helpers;

/// <summary>
/// Defines an xUnit test collection for integration tests that require the <see cref="WebServiceFixture"/>.
/// This collection ensures that all tests share the same web application and database instance while maintaining proper isolation.
/// </summary>
/// <remarks>
/// This class serves as a marker for xUnit's collection feature, which allows multiple test classes
/// to share the same fixture instance. The <see cref="CollectionDefinitionAttribute"/> with 
/// <c>DisableParallelization = true</c> ensures that tests within this collection run sequentially,
/// which is essential when using TestContainers for database integration testing.
/// 
/// Key characteristics:
/// <list type="bullet">
/// <item><strong>Sequential execution:</strong> Tests run one at a time to prevent database conflicts</item>
/// <item><strong>Shared fixture:</strong> All test classes in the collection use the same <see cref="WebServiceFixture"/> instance</item>
/// <item><strong>Resource optimization:</strong> Database container and web application are created once per test run</item>
/// <item><strong>Clean isolation:</strong> Each test gets a fresh database state through proper fixture lifecycle management</item>
/// </list>
/// 
/// The collection is designed for integration tests that need to:
/// <list type="bullet">
/// <item>Test HTTP endpoints with a real web application</item>
/// <item>Verify database interactions using a PostgreSQL TestContainer</item>
/// <item>Access the application's service provider for dependency resolution</item>
/// <item>Test authentication and authorization scenarios</item>
/// <item>Validate end-to-end workflows across multiple application layers</item>
/// </list>
/// 
/// Performance considerations:
/// <list type="bullet">
/// <item>The fixture is expensive to create (starts containers, applies migrations)</item>
/// <item>Sequential execution prevents race conditions but increases total test time</item>
/// <item>Shared fixture amortizes setup costs across multiple test classes</item>
/// <item>Database state is reset between test classes, not individual tests</item>
/// </list>
/// </remarks>
/// <example>
/// Using the collection in test classes:
/// <code>
/// [Collection(WebServiceCollection.Name)]
/// public class HomeControllerTests
/// {
///     private readonly WebServiceFixture _fixture;
/// 
///     public HomeControllerTests(WebServiceFixture fixture)
///     {
///         _fixture = fixture;
///     }
/// 
///     [Fact]
///     public async Task GetHome_ReturnsSuccessStatusCode()
///     {
///         // Arrange
///         using var client = _fixture.CreateClient();
/// 
///         // Act
///         var response = await client.GetAsync("/");
/// 
///         // Assert
///         response.Should().BeSuccessful();
///     }
/// }
/// 
/// [Collection(WebServiceCollection.Name)]
/// public class ApiControllerTests
/// {
///     private readonly WebServiceFixture _fixture;
/// 
///     public ApiControllerTests(WebServiceFixture fixture)
///     {
///         _fixture = fixture;
///     }
/// 
///     [Fact]
///     public async Task GetApi_RequiresAuthentication()
///     {
///         // Arrange
///         using var client = _fixture.CreateClient();
/// 
///         // Act
///         var response = await client.GetAsync("/api/secure");
/// 
///         // Assert
///         response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="WebServiceFixture"/>
/// <seealso cref="DatabaseFixture"/>
/// <seealso cref="CollectionDefinitionAttribute"/>
[CollectionDefinition(Name, DisableParallelization = true)]
public class SerializedCollection
{
    /// <summary>
    /// The name identifier for this test collection, used by test classes to join the collection.
    /// </summary>
    /// <value>
    /// A string constant that equals the class name "WebServiceCollection".
    /// </value>
    /// <remarks>
    /// This constant provides a strongly-typed way for test classes to reference the collection
    /// name in their <see cref="CollectionAttribute"/> declarations. Using <c>nameof</c> ensures
    /// that renaming the class will automatically update all references, reducing maintenance burden.
    /// 
    /// Test classes should use this constant rather than string literals:
    /// <c>[Collection(WebServiceCollection.Name)]</c> instead of <c>[Collection("WebServiceCollection")]</c>
    /// </remarks>
    /// <example>
    /// Referencing the collection in test classes:
    /// <code>
    /// [Collection(WebServiceCollection.Name)]
    /// public class MyIntegrationTests
    /// {
    ///     // Test implementation
    /// }
    /// </code>
    /// </example>
    public const string Name = nameof(SerializedCollection);
}
