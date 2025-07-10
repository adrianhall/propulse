using System.ComponentModel.DataAnnotations;
using System.Reflection;
using AwesomeAssertions;
using Propulse.Core.DataAnnotations;

namespace Propulse.Core.Tests.DataAnnotations;

/// <summary>
/// Unit tests for the <see cref="PropulseEmailAddressAttribute"/> validation attribute.
/// </summary>
public class PropulseEmailAddressAttributeTests
{
    private readonly PropulseEmailAddressAttribute _attribute = new();

    /// <summary>
    /// Test model to validate the attribute works correctly when applied to properties.
    /// </summary>
    private class TestModel
    {
        [PropulseEmailAddress]
        public string? Email { get; set; }
    }

    [Theory]
    [InlineData("john.doe@example.com", true)]
    [InlineData("jane-doe@sub.example.com", true)]
    [InlineData("user_name@xn--d1acufc.xn--p1ai", true)] // punycode domain
    [InlineData("a.b-c_d@a-b.c-d.com", true)]
    [InlineData("a@b.co", true)] // minimal valid segments
    [InlineData("a.b@a.b.c.d.e.f", true)] // 6 segments
    [InlineData("user@ex-ample.com", true)]
    [InlineData("user@ex.ample.com", true)]
    [InlineData("user@xn--bcher-kva.ch", true)] // IDN
    [InlineData("user@bücher.ch", true)] // Unicode domain
    [InlineData("user@a.b", true)] // minimal valid domain
    [InlineData("user@a.b.c.d.e.f", true)] // maximal valid domain
    public void IsValid_ValidEmails_ReturnsTrue(string email, bool expected)
    {
        var result = _attribute.IsValid(email);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("john..doe@example.com", false)] // consecutive dots in username
    [InlineData(".johndoe@example.com", true)] // leading dot is allowed
    [InlineData("johndoe.@example.com", true)] // trailing dot is allowed
    [InlineData("john#doe@example.com", false)] // invalid char in username
    [InlineData("john.doe@.example.com", false)] // empty segment
    [InlineData("john.doe@example..com", false)] // empty segment
    [InlineData("john.doe@example", false)] // only one segment
    [InlineData("john.doe@example.com.", false)] // trailing dot creates empty segment
    [InlineData("john.doe@.com", false)] // leading dot creates empty segment
    [InlineData("john.doe@com", false)] // only one segment
    [InlineData("john.doe@example.c", true)] // single char segment is valid
    [InlineData("john.doe@example.com.org.net.edu.gov.uk", false)] // 7 segments (too many)
    [InlineData("john.doe@", false)] // missing domain
    [InlineData("@example.com", false)] // missing username
    [InlineData("john.doeexample.com", false)] // missing @
    [InlineData("john.doe@@example.com", false)] // double @
    [InlineData("john.doe@xn--", false)] // invalid punycode
    [InlineData("john.doe@xn--.com", false)] // invalid punycode segment
    [InlineData("john.doe@bücher..ch", false)] // empty segment with unicode
    public void IsValid_InvalidEmails_ReturnsFalse(string email, bool expected)
    {
        var result = _attribute.IsValid(email);
        result.Should().Be(expected);
    }

    [Fact]
    public void IsValid_NullValue_ReturnsSuccess()
    {
        var result = _attribute.IsValid(null);
        result.Should().BeTrue("null values are considered valid by design");
    }

    [Fact]
    public void ValidationContext_WithModel_ValidatesCorrectly()
    {
        var model = new TestModel { Email = "john.doe@example.com" };
        var validationContext = new ValidationContext(model) { MemberName = nameof(TestModel.Email) };
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);

        isValid.Should().BeTrue("valid email should pass validation");
        validationResults.Should().BeEmpty("no validation errors should occur for valid input");
    }

    [Fact]
    public void ValidationContext_WithInvalidModel_ReturnsValidationError()
    {
        var model = new TestModel { Email = "john..doe@example.com" };
        var validationContext = new ValidationContext(model) { MemberName = nameof(TestModel.Email) };
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);

        isValid.Should().BeFalse("invalid email should fail validation");
        validationResults.Should().HaveCount(1, "exactly one validation error should occur");
        validationResults[0].ErrorMessage.Should().NotBeNullOrEmpty("error message should be provided");
    }

    [Theory]
    [InlineData("a@b.cd", true)]
    [InlineData("a@b.c.d", true)]
    [InlineData("a@b.c.d.e", true)]
    [InlineData("a@b.c.d.e.f", true)]
    [InlineData("a@b.c.d.e.f.g", true)]
    [InlineData("a@b.c.d.e.f.g.h", false)] // 7 segments
    public void IsValid_DomainSegmentCount_Boundaries(string email, bool expected)
    {
        var result = _attribute.IsValid(email);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("user@bücher.ch", true)]
    [InlineData("user@xn--bcher-kva.ch", true)]
    [InlineData("user@xn--bücher.ch", false)] // invalid punycode
    public void IsValid_InternationalizedDomainNames(string email, bool expected)
    {
        var result = _attribute.IsValid(email);
        result.Should().Be(expected);
    }

    [Fact]
    public void IsValid_WithValidationContext_NotString_ReturnsFalse()
    {
        // Arrange
        int testValue = 42;
        ValidationContext context = new(testValue);

        // Act
        var result = _attribute.GetValidationResult(testValue, context);

        // Assert
        result.Should().NotBeEquivalentTo(ValidationResult.Success);

    }

    [Fact]
    public void IsValid_WithValidationContext_EmptyString_ReturnsFalse()
    {
        // Arrange
        string testValue = string.Empty;
        ValidationContext context = new(testValue);
        
        // Act
        var result = _attribute.GetValidationResult(testValue, context);

        // Assert
        result.Should().NotBeEquivalentTo(ValidationResult.Success);
    }
}
