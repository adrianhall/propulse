namespace Propulse.Migrations.Tests.Helpers;

/// <summary>
/// Defines a test collection for migration tests that must run sequentially.
/// </summary>
/// <remarks>
/// This collection is used to group migration tests that cannot run in parallel due to shared database state.
/// Tests in this collection will be executed one at a time to prevent conflicts when applying database migrations.
/// This is particularly important when using TestContainers or shared database instances.
/// </remarks>
[CollectionDefinition(Name, DisableParallelization = true)]
public class MigrationCollection
{
    /// <summary>
    /// The name of the test collection used for grouping migration tests.
    /// </summary>
    /// <value>
    /// The collection name, which is the same as the class name.
    /// </value>
    /// <remarks>
    /// This constant is used both in the <see cref="CollectionDefinitionAttribute"/> and 
    /// in test classes that need to be part of this collection via the <c>[Collection]</c> attribute.
    /// </remarks>
    public const string Name = nameof(MigrationCollection);
}