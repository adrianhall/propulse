namespace Propulse.Web.Tests.Helpers;

/// <summary>
/// Defines an xUnit test collection for database-only integration tests that require the <see cref="DatabaseFixture"/>.
/// This collection is optimized for tests that only need database access without the full web application stack.
/// </summary>
/// <remarks>
/// This class serves as a marker for xUnit's collection feature for database-specific tests.
/// It provides similar sequential execution guarantees as SerializedCollection but with lower
/// overhead since it doesn't start the web application or HTTP client infrastructure.
/// 
/// Key characteristics:
/// <list type="bullet">
/// <item><strong>Sequential execution:</strong> Tests run one at a time to prevent database conflicts</item>
/// <item><strong>Shared database fixture:</strong> All test classes use the same <see cref="DatabaseFixture"/> instance</item>
/// <item><strong>Lightweight:</strong> No web application startup overhead</item>
/// <item><strong>Database-focused:</strong> Optimized for Entity Framework and direct database testing</item>
/// </list>
/// 
/// Use this collection for:
/// <list type="bullet">
/// <item>Entity Framework context testing</item>
/// <item>Database schema validation</item>
/// <item>Database migration testing</item>
/// <item>Repository pattern testing</item>
/// </list>
/// 
/// For tests that need HTTP client access, use <see cref="SerializedCollection"/> instead.
/// </remarks>
/// <example>
/// Using the collection in test classes:
/// <code>
/// [Collection(DatabaseCollection.Name)]
/// public class MyDatabaseTests
/// {
///     private readonly DatabaseFixture _fixture;
/// 
///     public MyDatabaseTests(DatabaseFixture fixture)
///     {
///         _fixture = fixture;
///     }
/// 
///     [Fact]
///     public async Task CanQueryDatabase()
///     {
///         // Use _fixture.ConnectionString for database operations
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="DatabaseFixture"/>
/// <seealso cref="SerializedCollection"/>
[CollectionDefinition(Name, DisableParallelization = true)]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    /// <summary>
    /// The name identifier for this test collection, used by test classes to join the collection.
    /// </summary>
    /// <value>
    /// A string constant that equals the class name "DatabaseCollection".
    /// </value>
    /// <remarks>
    /// This constant provides a strongly-typed way for test classes to reference the collection
    /// name in their <see cref="CollectionAttribute"/> declarations.
    /// </remarks>
    /// <example>
    /// Referencing the collection in test classes:
    /// <code>
    /// [Collection(DatabaseCollection.Name)]
    /// public class MyDatabaseTests
    /// {
    ///     // Test implementation
    /// }
    /// </code>
    /// </example>
    public const string Name = nameof(DatabaseCollection);
}
