using AwesomeAssertions;
using Propulse.Persistence.Tests.Helpers;

namespace Propulse.Persistence.Tests;

/// <summary>
/// Tests to validate that the database schema has been correctly created and matches
/// the expected ASP.NET Core Identity table structure in the security schema.
/// </summary>
public class DatabaseSchemaTests(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>
{
    /// <summary>
    /// Verifies that the security schema exists in the database.
    /// </summary>
    [Fact]
    public void SecuritySchemaExists()
    {
        // Act
        var tables = fixture.GetTableNames("security");

        // Assert
        tables.Should().NotBeNull("the security schema should exist and return a table list");
        tables.Should().NotBeEmpty("the security schema should contain Identity tables");
    }

    /// <summary>
    /// Verifies that all required ASP.NET Core Identity tables exist in the security schema
    /// with the correct naming conventions.
    /// </summary>
    [Fact]
    public void SecurityTablesExist()
    {
        // Arrange
        var expectedTables = new[]
        {
            "Roles",
            "RoleClaims", 
            "Users",
            "UserClaims",
            "UserLogins",
            "UserRoles",
            "UserTokens"
        };

        // Act
        var actualTables = fixture.GetTableNames("security");

        // Assert
        actualTables.Should().NotBeNullOrEmpty("the security schema should exist with the correct tables");
        var missingTables = expectedTables.Except(actualTables).ToList();
        missingTables.Should().BeEmpty($"the following required tables are missing: {string.Join(", ", missingTables)}");
    }

    /// <summary>
    /// Verifies that the Roles table has the correct structure with all required columns
    /// and appropriate data types for ASP.NET Core Identity.
    /// </summary>
    [Fact]
    public void RolesTableHasCorrectStructure()
    {
        // Arrange
        var expectedColumns = new Dictionary<string, string>
        {
            ["Id"] = "uuid",
            ["Name"] = "user-defined", // citext appears as user-defined
            ["NormalizedName"] = "user-defined", // citext appears as user-defined
            ["ConcurrencyStamp"] = "text"
        };

        // Act
        var actualColumns = fixture.GetColumnNamesAndTypes("Roles", "security").ToDictionary(x => x.Item1, x => x.Item2);

        // Assert
        actualColumns.Should().NotBeNullOrEmpty("the Roles table should exist");
        
        var missingColumns = expectedColumns.Keys.Except(actualColumns.Keys).ToList();
        missingColumns.Should().BeEmpty($"the following required columns are missing from Roles table: {string.Join(", ", missingColumns)}");

        foreach (var (columnName, expectedDataType) in expectedColumns)
        {
            actualColumns.Should().ContainKey(columnName, $"the Roles table should contain the column {columnName}");
            actualColumns[columnName].Should().BeEquivalentTo(expectedDataType, $"the Roles table column {columnName} should have type {expectedDataType}");
        }
    }

    /// <summary>
    /// Verifies that the Users table has the correct structure with all required columns
    /// and appropriate data types for ASP.NET Core Identity.
    /// </summary>
    [Fact]
    public void UsersTableHasCorrectStructure()
    {
        // Arrange
        var expectedColumns = new Dictionary<string, string>
        {
            ["Id"] = "uuid",
            ["UserName"] = "user-defined", // citext appears as user-defined
            ["NormalizedUserName"] = "user-defined", 
            ["Email"] = "user-defined",
            ["NormalizedEmail"] = "user-defined",
            ["EmailConfirmed"] = "boolean",
            ["PasswordHash"] = "text",
            ["SecurityStamp"] = "text",
            ["ConcurrencyStamp"] = "text",
            ["PhoneNumber"] = "text",
            ["PhoneNumberConfirmed"] = "boolean",
            ["TwoFactorEnabled"] = "boolean",
            ["LockoutEnd"] = "timestamp with time zone",
            ["LockoutEnabled"] = "boolean",
            ["AccessFailedCount"] = "integer"
        };

        // Act
        var actualColumns = fixture.GetColumnNamesAndTypes("Users", "security").ToDictionary(x => x.Item1, x => x.Item2);

        // Assert
        actualColumns.Should().NotBeNullOrEmpty("the Users table should exist");
        
        var missingColumns = expectedColumns.Keys.Except(actualColumns.Keys).ToList();
        missingColumns.Should().BeEmpty($"the following required columns are missing from Users table: {string.Join(", ", missingColumns)}");

        foreach (var (columnName, expectedDataType) in expectedColumns)
        {
            actualColumns.Should().ContainKey(columnName, $"the Users table should contain the column {columnName}");
            actualColumns[columnName].Should().BeEquivalentTo(expectedDataType, $"the Users table column {columnName} should have type {expectedDataType}");
        }
    }

    /// <summary>
    /// Verifies that the UserRoles junction table has the correct structure
    /// for the many-to-many relationship between users and roles.
    /// </summary>
    [Fact]
    public void UserRolesTableHasCorrectStructure()
    {
        // Arrange
        var expectedColumns = new Dictionary<string, string>
        {
            ["UserId"] = "uuid",
            ["RoleId"] = "uuid"
        };

        // Act
        var actualColumns = fixture.GetColumnNamesAndTypes("UserRoles", "security").ToDictionary(x => x.Item1, x => x.Item2);

        // Assert
        actualColumns.Should().NotBeNullOrEmpty("the UserRoles table should exist");
        
        var missingColumns = expectedColumns.Keys.Except(actualColumns.Keys).ToList();
        missingColumns.Should().BeEmpty($"the following required columns are missing from UserRoles table: {string.Join(", ", missingColumns)}");

        foreach (var (columnName, expectedDataType) in expectedColumns)
        {
            actualColumns.Should().ContainKey(columnName, $"the UserRoles table should contain the column {columnName}");
            actualColumns[columnName].Should().BeEquivalentTo(expectedDataType, $"the UserRoles table column {columnName} should have type {expectedDataType}");
        }
    }

    /// <summary>
    /// Verifies that the RoleClaims table has the correct structure
    /// for storing role-based authorization claims.
    /// </summary>
    [Fact]
    public void RoleClaimsTableHasCorrectStructure()
    {
        // Arrange
        var expectedColumns = new Dictionary<string, string>
        {
            ["Id"] = "integer",
            ["RoleId"] = "uuid",
            ["ClaimType"] = "text",
            ["ClaimValue"] = "text"
        };

        // Act
        var actualColumns = fixture.GetColumnNamesAndTypes("RoleClaims", "security").ToDictionary(x => x.Item1, x => x.Item2);

        // Assert
        actualColumns.Should().NotBeNullOrEmpty("the RoleClaims table should exist");
        
        var missingColumns = expectedColumns.Keys.Except(actualColumns.Keys).ToList();
        missingColumns.Should().BeEmpty($"the following required columns are missing from RoleClaims table: {string.Join(", ", missingColumns)}");

        foreach (var (columnName, expectedDataType) in expectedColumns)
        {
            actualColumns.Should().ContainKey(columnName, $"the RoleClaims table should contain the column {columnName}");
            actualColumns[columnName].Should().BeEquivalentTo(expectedDataType, $"the RoleClaims table column {columnName} should have type {expectedDataType}");
        }
    }

    /// <summary>
    /// Verifies that the UserClaims table has the correct structure
    /// for storing user-based authorization claims.
    /// </summary>
    [Fact]
    public void UserClaimsTableHasCorrectStructure()
    {
        // Arrange
        var expectedColumns = new Dictionary<string, string>
        {
            ["Id"] = "integer",
            ["UserId"] = "uuid",
            ["ClaimType"] = "text",
            ["ClaimValue"] = "text"
        };

        // Act
        var actualColumns = fixture.GetColumnNamesAndTypes("UserClaims", "security").ToDictionary(x => x.Item1, x => x.Item2);

        // Assert
        actualColumns.Should().NotBeNullOrEmpty("the UserClaims table should exist");
        
        var missingColumns = expectedColumns.Keys.Except(actualColumns.Keys).ToList();
        missingColumns.Should().BeEmpty($"the following required columns are missing from UserClaims table: {string.Join(", ", missingColumns)}");

        foreach (var (columnName, expectedDataType) in expectedColumns)
        {
            actualColumns.Should().ContainKey(columnName, $"the UserClaims table should contain the column {columnName}");
            actualColumns[columnName].Should().BeEquivalentTo(expectedDataType, $"the UserClaims table column {columnName} should have type {expectedDataType}");
        }
    }

    /// <summary>
    /// Verifies that the UserLogins table has the correct structure
    /// for storing external authentication provider login information.
    /// </summary>
    [Fact]
    public void UserLoginsTableHasCorrectStructure()
    {
        // Arrange
        var expectedColumns = new Dictionary<string, string>
        {
            ["LoginProvider"] = "text",
            ["ProviderKey"] = "text",
            ["ProviderDisplayName"] = "text",
            ["UserId"] = "uuid"
        };

        // Act
        var actualColumns = fixture.GetColumnNamesAndTypes("UserLogins", "security").ToDictionary(x => x.Item1, x => x.Item2);

        // Assert
        actualColumns.Should().NotBeNullOrEmpty("the UserLogins table should exist");
        
        var missingColumns = expectedColumns.Keys.Except(actualColumns.Keys).ToList();
        missingColumns.Should().BeEmpty($"the following required columns are missing from UserLogins table: {string.Join(", ", missingColumns)}");

        foreach (var (columnName, expectedDataType) in expectedColumns)
        {
            actualColumns.Should().ContainKey(columnName, $"the UserLogins table should contain the column {columnName}");
            actualColumns[columnName].Should().BeEquivalentTo(expectedDataType, $"the UserLogins table column {columnName} should have type {expectedDataType}");
        }
    }

    /// <summary>
    /// Verifies that the UserTokens table has the correct structure
    /// for storing authentication tokens from external providers.
    /// </summary>
    [Fact]
    public void UserTokensTableHasCorrectStructure()
    {
        // Arrange
        var expectedColumns = new Dictionary<string, string>
        {
            ["UserId"] = "uuid",
            ["LoginProvider"] = "text",
            ["Name"] = "text",
            ["Value"] = "text"
        };

        // Act
        var actualColumns = fixture.GetColumnNamesAndTypes("UserTokens", "security").ToDictionary(x => x.Item1, x => x.Item2);

        // Assert
        actualColumns.Should().NotBeNullOrEmpty("the UserTokens table should exist");
        
        var missingColumns = expectedColumns.Keys.Except(actualColumns.Keys).ToList();
        missingColumns.Should().BeEmpty($"the following required columns are missing from UserTokens table: {string.Join(", ", missingColumns)}");

        foreach (var (columnName, expectedDataType) in expectedColumns)
        {
            actualColumns.Should().ContainKey(columnName, $"the UserTokens table should contain the column {columnName}");
            actualColumns[columnName].Should().BeEquivalentTo(expectedDataType, $"the UserTokens table column {columnName} should have type {expectedDataType}");
        }
    }
}