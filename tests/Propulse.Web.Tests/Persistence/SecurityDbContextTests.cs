using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Propulse.Web.Entities;
using Propulse.Web.Persistence;
using Propulse.Web.Tests.Helpers;

namespace Propulse.Web.Tests.Persistence;

/// <summary>
/// Integration tests for SecurityDbContext with ApplicationUser and ApplicationRole.
/// </summary>
[Collection(SerializedCollection.Name)]
public class SecurityDbContextTests(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>
{
    #region Helper Methods

    /// <summary>
    /// Creates a new <see cref="SecurityDbContext"/> that connects
    /// to the database fixture.
    /// </summary>
    private SecurityDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SecurityDbContext>()
            .UseNpgsql(fixture.ConnectionString)
            .Options;

        return new SecurityDbContext(options);
    }
    
    #endregion

    [Fact]
    public async Task CanAddAndRetrieveUser()
    {
        var user = new ApplicationUser("integration.user@example.com");

        await using (var context = CreateContext())
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var retrieved = await context.Users.SingleOrDefaultAsync(u => u.Id == user.Id);
            retrieved.Should().NotBeNull("user should be persisted and retrievable");
            retrieved!.UserName.Should().Be(user.UserName);
            retrieved.Email.Should().Be(user.Email);
            retrieved.NormalizedUserName.Should().Be(user.NormalizedUserName);
            retrieved.NormalizedEmail.Should().Be(user.NormalizedEmail);
        }
    }

    [Fact]
    public async Task CanAddAndRetrieveRole()
    {
        var role = new ApplicationRole("IntegrationRole");

        await using (var context = CreateContext())
        {
            context.Roles.Add(role);
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var retrieved = await context.Roles.SingleOrDefaultAsync(r => r.Id == role.Id);
            retrieved.Should().NotBeNull("role should be persisted and retrievable");
            retrieved!.Name.Should().Be(role.Name);
            retrieved.NormalizedName.Should().Be(role.NormalizedName);
        }
    }

    [Fact]
    public async Task CanRemoveUser()
    {
        var user = new ApplicationUser("delete.user@example.com");

        await using (var context = CreateContext())
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var toRemove = await context.Users.SingleAsync(u => u.Id == user.Id);
            context.Users.Remove(toRemove);
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var exists = await context.Users.AnyAsync(u => u.Id == user.Id);
            exists.Should().BeFalse("user should be deleted from the database");
        }
    }

    [Fact]
    public async Task CanRemoveRole()
    {
        var role = new ApplicationRole("DeleteRole");

        await using (var context = CreateContext())
        {
            context.Roles.Add(role);
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var toRemove = await context.Roles.SingleAsync(r => r.Id == role.Id);
            context.Roles.Remove(toRemove);
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var exists = await context.Roles.AnyAsync(r => r.Id == role.Id);
            exists.Should().BeFalse("role should be deleted from the database");
        }
    }

    [Fact]
    public async Task CanAddUserAndRoleAndAssignRole()
    {
        var user = new ApplicationUser("assign.role@example.com");
        var role = new ApplicationRole("AssignedRole");

        await using (var context = CreateContext())
        {
            context.Users.Add(user);
            context.Roles.Add(role);
            await context.SaveChangesAsync();

            // Assign role to user via UserRoles join table
            context.UserRoles.Add(new Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>
            {
                UserId = user.Id,
                RoleId = role.Id
            });
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var userRoles = await context.UserRoles
                .Where(ur => ur.UserId == user.Id && ur.RoleId == role.Id)
                .ToListAsync();

            userRoles.Should().NotBeEmpty("user should be assigned to the role");
        }
    }
}
