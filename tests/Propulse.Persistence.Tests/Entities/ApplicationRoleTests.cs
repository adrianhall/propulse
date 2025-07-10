using AwesomeAssertions;
using Propulse.Persistence.Entities;

namespace Propulse.Persistence.Tests.Entities;

/// <summary>
/// Unit tests for the ApplicationRole entity class.
/// </summary>
public class ApplicationRoleTests
{
    [Fact]
    public void DefaultConstructor_CreatesInstanceWithGeneratedId()
    {
        // Act
        var role = new ApplicationRole();

        // Assert
        role.Id.Should().NotBe(Guid.Empty, "Id should be automatically generated");
        role.Id.Should().NotBe(default(Guid), "Id should not be default Guid");
        role.Name.Should().BeNull("Name should be null by default");
        role.NormalizedName.Should().BeNull("NormalizedName should be null by default");
    }

    [Fact]
    public void DefaultConstructor_GeneratesUniqueIds()
    {
        // Act
        var role1 = new ApplicationRole();
        var role2 = new ApplicationRole();

        // Assert
        role1.Id.Should().NotBe(role2.Id, "each instance should have a unique Id");
        role1.Id.Should().NotBe(Guid.Empty, "first role Id should not be empty");
        role2.Id.Should().NotBe(Guid.Empty, "second role Id should not be empty");
    }

    [Theory]
    [InlineData("Administrator")]
    [InlineData("User")]
    [InlineData("Manager")]
    [InlineData("SuperAdmin")]
    [InlineData("ContentEditor")]
    [InlineData("SystemAdministrator")]
    public void ParameterizedConstructor_ValidRoleName_CreatesInstanceCorrectly(string roleName)
    {
        // Act
        var role = new ApplicationRole(roleName);

        // Assert
        role.Id.Should().NotBe(Guid.Empty, "Id should be automatically generated");
        role.Id.Should().NotBe(default(Guid), "Id should not be default Guid");
        role.Name.Should().Be(roleName, "Name should match the provided role name");
        role.NormalizedName.Should().Be(roleName.ToUpperInvariant(), "NormalizedName should be uppercase version of role name");
    }

    [Fact]
    public void ParameterizedConstructor_ValidRoleName_GeneratesUniqueIds()
    {
        // Arrange
        const string roleName = "TestRole";

        // Act
        var role1 = new ApplicationRole(roleName);
        var role2 = new ApplicationRole(roleName);

        // Assert
        role1.Id.Should().NotBe(role2.Id, "each instance should have a unique Id even with same role name");
        role1.Name.Should().Be(role2.Name, "both roles should have the same name");
        role1.NormalizedName.Should().Be(role2.NormalizedName, "both roles should have the same normalized name");
    }

    [Fact]
    public void ParameterizedConstructor_NullRoleName_ThrowsArgumentNullException()
    {
        // Arrange
        Action act = () => _ = new ApplicationRole(null!);

        // Act & Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("roleName");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void ParameterizedConstructor_EmptyOrWhitespaceRoleName_ThrowsArgumentException(string roleName)
    {
        // Arrange
        Action act = () => _ = new ApplicationRole(roleName);

        // Act & Assert
        act.Should().Throw<ArgumentException>().WithParameterName("roleName");
    }

    [Theory]
    [InlineData("admin")] // Starts with lowercase
    [InlineData("Ad")] // Too short
    [InlineData("A")] // Too short
    [InlineData("Admin123")] // Contains numbers
    [InlineData("Admin-User")] // Contains hyphen
    [InlineData("Admin_User")] // Contains underscore
    [InlineData("Admin User")] // Contains space
    [InlineData("Admin@User")] // Contains special character
    public void ParameterizedConstructor_InvalidRoleName_ThrowsArgumentException(string invalidRoleName)
    {
        // Arrange
        Action act = () => _ = new ApplicationRole(invalidRoleName);

        // Act & Assert
        act.Should().Throw<ArgumentException>().WithParameterName("roleName");
    }

    [Fact]
    public void ParameterizedConstructor_MaxLengthRoleName_CreatesInstanceCorrectly()
    {
        // Arrange - Create a role name with exactly 64 characters (maximum allowed)
        var maxLengthRoleName = "A" + new string('b', 63); // 64 total characters

        // Act
        var role = new ApplicationRole(maxLengthRoleName);

        // Assert
        role.Id.Should().NotBe(Guid.Empty, "Id should be automatically generated");
        role.Name.Should().Be(maxLengthRoleName, "Name should match the provided role name");
        role.NormalizedName.Should().Be(maxLengthRoleName.ToUpperInvariant(), "NormalizedName should be uppercase version");
    }

    [Fact]
    public void ParameterizedConstructor_ExceedsMaxLength_ThrowsArgumentException()
    {
        // Arrange
        var tooLongRoleName = "A" + new string('b', 64); // 65 total characters
        Action act = () => _ = new ApplicationRole(tooLongRoleName);

        // Act & Assert
        act.Should().Throw<ArgumentException>().WithParameterName("roleName");
    }

    [Theory]
    [InlineData("Administrator", "ADMINISTRATOR")]
    [InlineData("User", "USER")]
    [InlineData("ContentManager", "CONTENTMANAGER")]
    [InlineData("SystemOperator", "SYSTEMOPERATOR")]
    [InlineData("ABC", "ABC")]
    [InlineData("Admin", "ADMIN")]
    public void ParameterizedConstructor_NormalizationBehavior_ConvertsToUppercase(string input, string expectedNormalized)
    {
        // Act
        var role = new ApplicationRole(input);

        // Assert
        role.Name.Should().Be(input, "original name should be preserved");
        role.NormalizedName.Should().Be(expectedNormalized, "normalized name should be converted to uppercase");
    }

    [Fact]
    public void ParameterizedConstructor_MixedCaseRoleName_PreservesOriginalCase()
    {
        // Arrange
        const string mixedCaseRoleName = "DatabaseAdministrator";

        // Act
        var role = new ApplicationRole(mixedCaseRoleName);

        // Assert
        role.Name.Should().Be(mixedCaseRoleName, "original case should be preserved in Name property");
        role.NormalizedName.Should().Be("DATABASEADMINISTRATOR", "NormalizedName should be all uppercase");
    }

    [Fact]
    public void InheritsFromIdentityRole_WithGuidKey()
    {
        // Arrange & Act
        var role = new ApplicationRole();

        // Assert
        role.Should().BeAssignableTo<Microsoft.AspNetCore.Identity.IdentityRole<Guid>>(
            "ApplicationRole should inherit from IdentityRole<Guid>");
    }

    [Fact]
    public void DefaultConstructor_SetsInheritedPropertiesCorrectly()
    {
        // Act
        var role = new ApplicationRole();

        // Assert
        role.ConcurrencyStamp.Should().BeNull("ConcurrencyStamp is null by default in IdentityRole");
    }

    [Fact]
    public void ParameterizedConstructor_SetsInheritedPropertiesCorrectly()
    {
        // Arrange
        const string roleName = "TestRole";

        // Act
        var role = new ApplicationRole(roleName);

        // Assert
        role.ConcurrencyStamp.Should().NotBeNullOrEmpty("ConcurrencyStamp should be set by base class");
        role.Name.Should().Be(roleName, "Name should be set correctly");
        role.NormalizedName.Should().Be(roleName.ToUpperInvariant(), "NormalizedName should be set correctly");
    }

    [Fact]
    public void ToString_ReturnsRoleName()
    {
        // Arrange
        const string roleName = "Administrator";
        var role = new ApplicationRole(roleName);

        // Act
        var result = role.ToString();

        // Assert
        result.Should().Be(roleName, "ToString should return the role name");
    }

    [Fact]
    public void ToString_DefaultConstructor_ReturnsEmptyString()
    {
        // Arrange
        var role = new ApplicationRole();

        // Act
        var result = role.ToString();

        // Assert
        result.Should().BeEmpty("ToString should return empty string when Name is null");
    }

    [Theory]
    [InlineData("ADMIN")]
    [InlineData("USER")]
    [InlineData("MANAGER")]
    public void ParameterizedConstructor_AllUppercaseRoleName_HandledCorrectly(string uppercaseRoleName)
    {
        // Act
        var role = new ApplicationRole(uppercaseRoleName);

        // Assert
        role.Name.Should().Be(uppercaseRoleName, "original uppercase name should be preserved");
        role.NormalizedName.Should().Be(uppercaseRoleName, "normalized name should remain uppercase");
    }

    [Fact]
    public void ParameterizedConstructor_CallsBaseConstructor()
    {
        // Arrange
        const string roleName = "TestRole";

        // Act
        var role = new ApplicationRole(roleName);

        // Assert
        role.Id.Should().NotBe(Guid.Empty, "Id should be set by calling the default constructor");
        role.Name.Should().Be(roleName, "Name should be set by parameterized constructor");
        role.NormalizedName.Should().Be(roleName.ToUpperInvariant(), "NormalizedName should be set by parameterized constructor");
    }

    [Fact]
    public void RoleNamesWithDifferentCasing_ProduceSameNormalizedName()
    {
        // Arrange & Act
        var role1 = new ApplicationRole("Administrator");
        var role2 = new ApplicationRole("ADMINISTRATOR");

        // Assert
        role1.NormalizedName.Should().Be("ADMINISTRATOR", "first role should normalize correctly");
        role2.NormalizedName.Should().Be("ADMINISTRATOR", "second role should normalize correctly");
        role1.NormalizedName.Should().Be(role2.NormalizedName, "both roles should have the same normalized name");
        
        // Test that invalid casing throws exception
        Action act = () => _ = new ApplicationRole("administrator");
        act.Should().Throw<ArgumentException>().WithParameterName("roleName");
    }
}
