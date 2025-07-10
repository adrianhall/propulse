using System.ComponentModel.DataAnnotations;
using AwesomeAssertions;
using Propulse.Core.DataAnnotations;

namespace Propulse.Core.Tests.DataAnnotations;

/// <summary>
/// Unit tests for the PropulseRoleNameAttribute validation attribute.
/// </summary>
public class PropulseRoleNameAttributeTests
{
    private readonly PropulseRoleNameAttribute _attribute = new();

    /// <summary>
    /// Test model to validate the attribute works correctly when applied to properties.
    /// </summary>
    private class TestModel
    {
        [PropulseRoleName]
        public string? RoleName { get; set; }
    }

    [Theory]
    [InlineData("Admin", true)]
    [InlineData("User", true)]
    [InlineData("Manager", true)]
    [InlineData("SuperAdmin", true)]
    [InlineData("ContentEditor", true)]
    [InlineData("SystemAdministrator", true)]
    [InlineData("ABC", true)] // Minimum length (3 characters)
    [InlineData("ADMIN", true)] // All uppercase is valid per regex
    [InlineData("USER", true)] // All uppercase is valid per regex
    public void IsValid_ValidRoleNames_ReturnsTrue(string roleName, bool expected)
    {
        // Act
        var result = _attribute.IsValid(roleName);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("admin", false)] // Starts with lowercase
    [InlineData("Ad", false)] // Too short (2 characters)
    [InlineData("A", false)] // Too short (1 character)
    [InlineData("Admin123", false)] // Contains numbers
    [InlineData("Admin-User", false)] // Contains hyphen
    [InlineData("Admin_User", false)] // Contains underscore
    [InlineData("Admin User", false)] // Contains space
    [InlineData("Admin@User", false)] // Contains special character
    [InlineData("Ädmin", false)] // Contains non-ASCII character
    public void IsValid_InvalidRoleNames_ReturnsFalse(string roleName, bool expected)
    {
        // Act
        var result = _attribute.IsValid(roleName);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void IsValid_NullValue_ReturnsTrue()
    {
        // Act
        var result = _attribute.IsValid(null);

        // Assert
        result.Should().BeTrue("RegularExpressionAttribute returns true for null values by design");
    }

    [Fact]
    public void IsValid_EmptyString_ReturnsTrue()
    {
        // Act
        var result = _attribute.IsValid("");

        // Assert
        result.Should().BeTrue("RegularExpressionAttribute returns true for empty strings by design");
    }

    [Fact]
    public void IsValid_MaximumLengthRoleName_ReturnsTrue()
    {
        // Arrange - Create a role name with exactly 64 characters (maximum allowed)
        var maxLengthRoleName = "A" + new string('b', 63); // 64 total characters

        // Act
        var result = _attribute.IsValid(maxLengthRoleName);

        // Assert
        result.Should().BeTrue("role names up to 64 characters should be valid");
    }

    [Fact]
    public void IsValid_ExceedsMaximumLength_ReturnsFalse()
    {
        // Arrange - Create a role name with 65 characters (exceeds maximum)
        var tooLongRoleName = "A" + new string('b', 64); // 65 total characters

        // Act
        var result = _attribute.IsValid(tooLongRoleName);

        // Assert
        result.Should().BeFalse("role names exceeding 64 characters should be invalid");
    }

    [Fact]
    public void ValidationContext_WithModel_ValidatesCorrectly()
    {
        // Arrange
        var model = new TestModel { RoleName = "ValidRole" };
        var validationContext = new ValidationContext(model) { MemberName = nameof(TestModel.RoleName) };
        var validationResults = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);

        // Assert
        isValid.Should().BeTrue("valid role name should pass validation");
        validationResults.Should().BeEmpty("no validation errors should occur for valid input");
    }

    [Fact]
    public void ValidationContext_WithInvalidModel_ReturnsValidationError()
    {
        // Arrange
        var model = new TestModel { RoleName = "invalidRole" }; // Starts with lowercase
        var validationContext = new ValidationContext(model) { MemberName = nameof(TestModel.RoleName) };
        var validationResults = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);

        // Assert
        isValid.Should().BeFalse("invalid role name should fail validation");
        validationResults.Should().HaveCount(1, "exactly one validation error should occur");
        validationResults[0].ErrorMessage.Should().NotBeNullOrEmpty("error message should be provided");
        validationResults[0].MemberNames.Should().Contain(nameof(TestModel.RoleName), 
            "validation error should reference the correct property");
    }

    [Fact]
    public void AttributeUsage_HasCorrectSettings()
    {
        // Arrange
        var attributeType = typeof(PropulseRoleNameAttribute);

        // Act
        var attributeUsage = attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>()
            .First();

        // Assert
        attributeUsage.ValidOn.Should().Be(AttributeTargets.Property, 
            "attribute should only be applicable to properties");
        attributeUsage.AllowMultiple.Should().BeFalse(
            "multiple instances of the attribute should not be allowed on a single property");
        attributeUsage.Inherited.Should().BeTrue(
            "attribute should be inherited by derived classes");
    }

    [Fact]
    public void Inheritance_InheritsFromRegularExpressionAttribute()
    {
        // Act & Assert
        _attribute.Should().BeAssignableTo<RegularExpressionAttribute>(
            "PropulseRoleNameAttribute should inherit from RegularExpressionAttribute");
    }

    [Theory]
    [InlineData("Abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdef", true)] // 58 chars
    [InlineData("Abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefg", true)] // 59 chars
    [InlineData("Abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefgh", true)] // 60 chars
    [InlineData("Abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghi", true)] // 61 chars
    [InlineData("Abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghij", true)] // 62 chars
    [InlineData("Abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijk", true)] // 63 chars
    [InlineData("Abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijkl", true)] // 64 chars
    [InlineData("Abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklm", false)] // 65 chars
    public void IsValid_BoundaryLengthTests_ValidatesCorrectly(string roleName, bool expected)
    {
        // Act
        var result = _attribute.IsValid(roleName);

        // Assert
        result.Should().Be(expected, $"role name with {roleName.Length} characters should be {(expected ? "valid" : "invalid")}");
    }

    [Theory]
    [InlineData("Administrator")]
    [InlineData("ContentManager")]
    [InlineData("SystemOperator")]
    [InlineData("DataAnalyst")]
    [InlineData("ProjectLead")]
    [InlineData("QualityAssurance")]
    [InlineData("DatabaseAdministrator")]
    public void IsValid_CommonRoleNames_ReturnsTrue(string roleName)
    {
        // Act
        var result = _attribute.IsValid(roleName);

        // Assert
        result.Should().BeTrue($"common role name '{roleName}' should be valid");
    }
}
