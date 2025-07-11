using AwesomeAssertions;
using Propulse.Web.Tests.Helpers;

namespace Propulse.Web.Tests.Persistence;

[Collection(DatabaseCollection.Name)]
public class DatabaseSchemaTests(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>
{
    #region Table Definitions
    private static readonly Dictionary<string, string> RolesTableDefinition = new()
    {
        { "Id", "uuid" },
        { "Name", "citext" },
        { "NormalizedName", "citext" },
        { "ConcurrencyStamp", "text" }
    };

    private static readonly Dictionary<string, string> RoleClaimsTableDefinition = new()
    {
        { "Id", "integer" },
        { "RoleId", "uuid" },
        { "ClaimType", "text" },
        { "ClaimValue", "text" }
    };

    private static readonly Dictionary<string, string> UsersTableDefinition = new()
    {
        { "Id", "uuid" },
        { "UserName", "citext" },
        { "NormalizedUserName", "citext" },
        { "Email", "citext" },
        { "NormalizedEmail", "citext" },
        { "EmailConfirmed", "boolean" },
        { "PasswordHash", "text" },
        { "SecurityStamp", "text" },
        { "ConcurrencyStamp", "text" },
        { "PhoneNumber", "text" },
        { "PhoneNumberConfirmed", "boolean" },
        { "TwoFactorEnabled", "boolean" },
        { "LockoutEnd", "timestamp with time zone" },
        { "LockoutEnabled", "boolean" },
        { "AccessFailedCount", "integer" }
    };

    private static readonly Dictionary<string, string> UserRolesTableDefinition = new()
    {
        { "UserId", "uuid" },
        { "RoleId", "uuid" }
    };

    private static readonly Dictionary<string, string> UserClaimsTableDefinition = new()
    {
        { "Id", "integer" },
        { "UserId", "uuid" },
        { "ClaimType", "text" },
        { "ClaimValue", "text" }
    };

    private static readonly Dictionary<string, string> UserLoginsTableDefinition = new()
    {
        { "LoginProvider", "text" },
        { "ProviderKey", "text" },
        { "ProviderDisplayName", "text" },
        { "UserId", "uuid" }
    };

    private static readonly Dictionary<string, string> UserTokensTableDefinition = new()
    {
        { "UserId", "uuid" },
        { "LoginProvider", "text" },
        { "Name", "text" },
        { "Value", "text" }
    };
    #endregion

    [Fact]
    public void SecuritySchema_ShouldBeCorrectlyImplemented()
    {
        // Assert - Correct extensions should exist
        fixture.Should().HaveExtensions(["citext", "uuid-ossp"]);

        // Assert - Schema should be created
        fixture.Should().HaveSchema("security");

        // Assert - Correct tables should exist (complete with expected columns and column types)
        fixture.Should().HaveTableWithDefinition("Roles", "security", RolesTableDefinition);
        fixture.Should().HaveTableWithDefinition("RoleClaims", "security", RoleClaimsTableDefinition);
        fixture.Should().HaveTableWithDefinition("Users", "security", UsersTableDefinition);
        fixture.Should().HaveTableWithDefinition("UserRoles", "security", UserRolesTableDefinition);
        fixture.Should().HaveTableWithDefinition("UserClaims", "security", UserClaimsTableDefinition);
        fixture.Should().HaveTableWithDefinition("UserLogins", "security", UserLoginsTableDefinition);
        fixture.Should().HaveTableWithDefinition("UserTokens", "security", UserTokensTableDefinition);
    }
}
