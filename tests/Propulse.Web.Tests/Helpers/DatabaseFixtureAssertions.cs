using AwesomeAssertions;
using AwesomeAssertions.Execution;
using AwesomeAssertions.Primitives;

namespace Propulse.Web.Tests.Helpers;

internal static class  DatabaseFixtureExtensions
{
    public static DatabaseFixtureAssertions Should(this DatabaseFixture fixture)
    {
        return new DatabaseFixtureAssertions(fixture, AssertionChain.GetOrCreate());
    }
}

internal class DatabaseFixtureAssertions(DatabaseFixture fixture, AssertionChain chain)
    : ReferenceTypeAssertions<DatabaseFixture, DatabaseFixtureAssertions>(fixture, chain)
{
    protected override string Identifier => nameof(DatabaseFixture);

    [CustomAssertion]
    public AndConstraint<DatabaseFixtureAssertions> HaveExtensions(IEnumerable<string> expectedExtensions, string because = "", params object[] becauseArgs)
    {
        // Query the pg_extension system catalog to get installed extensions
        var queryResults = Subject.Query("SELECT extname FROM pg_extension");
        var installedExtensions = queryResults.Select(row => row["extname"]?.ToString() ?? string.Empty).ToList();
        var missingExtensions = expectedExtensions
            .Where(ext => !installedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
            .ToList();

        CurrentAssertionChain
            .ForCondition(missingExtensions.Count == 0)
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected database to have extensions {0}{reason}, but the following were missing: {1}",
                string.Join(", ", expectedExtensions), 
                string.Join(", ", missingExtensions));

        return new AndConstraint<DatabaseFixtureAssertions>(this);
    }

    [CustomAssertion]
    public AndConstraint<DatabaseFixtureAssertions> HaveSchema(string schemaName, string because = "", params object[] becauseArgs)
    {
        var queryResults = Subject.Query("SELECT nspname FROM pg_namespace WHERE nspname = @p0", ("p0", schemaName));

        CurrentAssertionChain
            .ForCondition(queryResults.Count > 0)
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected database to have schema '{0}'{reason}, but it was not found.", schemaName);

        return new AndConstraint<DatabaseFixtureAssertions>(this);
    }

    [CustomAssertion]
    public AndConstraint<DatabaseFixtureAssertions> HaveTableWithDefinition(string tableName, string schemaName, Dictionary<string, string> expectedColumns, string because = "", params object[] becauseArgs)
    {
        // Query to get the table definition
        var query = @"
            SELECT * 
            FROM information_schema.columns 
            WHERE table_schema = @p0 AND table_name = @p1";
        var queryResults = Subject.Query(query, ("p0", schemaName), ("p1", tableName));

        // Step 1: If there are no results, the table does not exist
        CurrentAssertionChain
            .ForCondition(queryResults.Count > 0)
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected table '{0}.{1}' to exist{reason}, but it was not found.", schemaName, tableName);

        // Convert the queryResults to a dictionary for easier access
        var actualColumns = queryResults.ToDictionary(
            row => row["column_name"]?.ToString() ?? string.Empty,
            row =>
            {
                var dataType = row["data_type"]?.ToString() ?? string.Empty;
                if (string.Equals(dataType, "USER-DEFINED", StringComparison.OrdinalIgnoreCase))
                {
                    // Use udt_name for user-defined types (e.g., citext)
                    return row["udt_name"]?.ToString() ?? string.Empty;
                }
                return dataType;
            });

        // Step 2: All the columns in the expected definition must exist in the actual results
        foreach (var expectedColumn in expectedColumns)
        {
            bool columnExists = actualColumns.TryGetValue(expectedColumn.Key, out var actualType);

            // Check if the expected column exists in the actual columns
            CurrentAssertionChain
                .ForCondition(columnExists)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected table '{0}.{1}' to have column '{2}'{reason}, but it was not found.",
                    schemaName, tableName, expectedColumn.Key);

            // Check if the actual type matches the expected type
            // Note that citext columns may appear as USER-DEFINED when using information_schema.columns
            CurrentAssertionChain
                .ForCondition(string.Equals(actualType, expectedColumn.Value, StringComparison.OrdinalIgnoreCase))
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected table '{0}.{1}' to have column '{2}' with type '{3}'{reason}, but found type '{4}'.",
                    schemaName, tableName, expectedColumn.Key, expectedColumn.Value, actualType);
        }

        return new AndConstraint<DatabaseFixtureAssertions>(this);
    }
}
