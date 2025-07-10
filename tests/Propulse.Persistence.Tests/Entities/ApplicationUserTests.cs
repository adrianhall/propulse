using System;
using AwesomeAssertions;
using Propulse.Persistence.Entities;

namespace Propulse.Persistence.Tests.Entities;

/// <summary>
/// Unit tests for the ApplicationUser entity class.
/// </summary>
public class ApplicationUserTests
{
    [Fact]
    public void DefaultConstructor_CreatesInstanceWithGeneratedId()
    {
        var user = new ApplicationUser();

        user.Id.Should().NotBe(Guid.Empty, "Id should be automatically generated");
        user.Id.Should().NotBe(default(Guid), "Id should not be default Guid");
        user.UserName.Should().BeNull("UserName should be null by default");
        user.Email.Should().BeNull("Email should be null by default");
        user.NormalizedUserName.Should().BeNull("NormalizedUserName should be null by default");
        user.NormalizedEmail.Should().BeNull("NormalizedEmail should be null by default");
    }

    [Fact]
    public void DefaultConstructor_GeneratesUniqueIds()
    {
        var user1 = new ApplicationUser();
        var user2 = new ApplicationUser();

        user1.Id.Should().NotBe(user2.Id, "each instance should have a unique Id");
        user1.Id.Should().NotBe(Guid.Empty, "first user Id should not be empty");
        user2.Id.Should().NotBe(Guid.Empty, "second user Id should not be empty");
    }

    [Theory]
    [InlineData("john.doe@example.com")]
    [InlineData("Jane.Doe@sub.example.com")]
    [InlineData("user_name@bücher.ch")]
    [InlineData("user@xn--bcher-kva.ch")]
    public void ParameterizedConstructor_ValidUserName_CreatesInstanceCorrectly(string userName)
    {
        var user = new ApplicationUser(userName);

        user.Id.Should().NotBe(Guid.Empty, "Id should be automatically generated");
        user.UserName.Should().Be(userName, "UserName should match the provided value");
        user.Email.Should().Be(userName, "Email should match the provided value");
        user.NormalizedUserName.Should().Be(userName.ToUpperInvariant(), "NormalizedUserName should be uppercase");
        user.NormalizedEmail.Should().Be(userName.ToUpperInvariant(), "NormalizedEmail should be uppercase");
    }

    [Fact]
    public void ParameterizedConstructor_ValidUserName_GeneratesUniqueIds()
    {
        const string userName = "john.doe@example.com";
        var user1 = new ApplicationUser(userName);
        var user2 = new ApplicationUser(userName);

        user1.Id.Should().NotBe(user2.Id, "each instance should have a unique Id even with same user name");
        user1.UserName.Should().Be(user2.UserName, "both users should have the same user name");
        user1.NormalizedUserName.Should().Be(user2.NormalizedUserName, "both users should have the same normalized user name");
    }

    [Fact]
    public void ParameterizedConstructor_NullUserName_ThrowsArgumentNullException()
    {
        Action act = () => _ = new ApplicationUser(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("userName");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void ParameterizedConstructor_EmptyOrWhitespaceUserName_ThrowsArgumentException(string userName)
    {
        Action act = () => _ = new ApplicationUser(userName);

        act.Should().Throw<ArgumentException>().WithParameterName("userName");
    }

    [Theory]
    [InlineData("john..doe@example.com")] // consecutive dots in username
    [InlineData("john#doe@example.com")] // invalid char in username
    [InlineData("john.doe@.example.com")] // empty segment
    [InlineData("john.doe@example..com")] // empty segment
    [InlineData("john.doe@example")] // only one segment
    [InlineData("john.doe@")] // missing domain
    [InlineData("@example.com")] // missing username
    [InlineData("john.doeexample.com")] // missing @
    [InlineData("john.doe@@example.com")] // double @
    [InlineData("john.doe@xn--")] // invalid punycode
    [InlineData("john.doe@xn--.com")] // invalid punycode segment
    [InlineData("john.doe@bücher..ch")] // empty segment with unicode
    public void ParameterizedConstructor_InvalidUserName_ThrowsArgumentException(string invalidUserName)
    {
        Action act = () => _ = new ApplicationUser(invalidUserName);

        act.Should().Throw<ArgumentException>().WithParameterName("userName");
    }

    [Fact]
    public void ParameterizedConstructor_MaxLengthUserName_CreatesInstanceCorrectly()
    {
        // 64 chars local part, 1 @, 2+ chars domain, total < 254
        var maxLengthUserName = "a" + new string('b', 62) + "@ex.com";
        var user = new ApplicationUser(maxLengthUserName);

        user.Id.Should().NotBe(Guid.Empty, "Id should be automatically generated");
        user.UserName.Should().Be(maxLengthUserName, "UserName should match the provided value");
        user.NormalizedUserName.Should().Be(maxLengthUserName.ToUpperInvariant(), "NormalizedUserName should be uppercase");
    }

    [Theory]
    [InlineData("john.doe@example.com", "JOHN.DOE@EXAMPLE.COM")]
    [InlineData("Jane.Doe@bücher.ch", "JANE.DOE@BÜCHER.CH")]
    public void ParameterizedConstructor_NormalizationBehavior_ConvertsToUppercase(string input, string expectedNormalized)
    {
        var user = new ApplicationUser(input);

        user.UserName.Should().Be(input, "original user name should be preserved");
        user.NormalizedUserName.Should().Be(expectedNormalized, "normalized user name should be converted to uppercase");
        user.Email.Should().Be(input, "original email should be preserved");
        user.NormalizedEmail.Should().Be(expectedNormalized, "normalized email should be converted to uppercase");
    }

    [Fact]
    public void ParameterizedConstructor_MixedCaseUserName_PreservesOriginalCase()
    {
        const string mixedCaseUserName = "Jane.Doe@Example.com";
        var user = new ApplicationUser(mixedCaseUserName);

        user.UserName.Should().Be(mixedCaseUserName, "original case should be preserved in UserName property");
        user.NormalizedUserName.Should().Be(mixedCaseUserName.ToUpperInvariant(), "NormalizedUserName should be all uppercase");
    }

    [Fact]
    public void InheritsFromIdentityUser_WithGuidKey()
    {
        var user = new ApplicationUser();

        user.Should().BeAssignableTo<Microsoft.AspNetCore.Identity.IdentityUser<Guid>>(
            "ApplicationUser should inherit from IdentityUser<Guid>");
    }

    [Fact]
    public void DefaultConstructor_SetsInheritedPropertiesCorrectly()
    {
        var user = new ApplicationUser();

        user.ConcurrencyStamp.Should().NotBeNullOrEmpty("ConcurrencyStamp is always defined by default in IdentityUser");
    }

    [Fact]
    public void ParameterizedConstructor_SetsInheritedPropertiesCorrectly()
    {
        const string userName = "john.doe@example.com";
        var user = new ApplicationUser(userName);

        user.ConcurrencyStamp.Should().NotBeNullOrEmpty("ConcurrencyStamp should be set by base class");
        user.UserName.Should().Be(userName, "UserName should be set correctly");
        user.NormalizedUserName.Should().Be(userName.ToUpperInvariant(), "NormalizedUserName should be set correctly");
    }

    [Fact]
    public void ToString_DefaultConstructor_ReturnsEmptyString()
    {
        var user = new ApplicationUser();

        var result = user.ToString();

        result.Should().BeEmpty("ToString should return empty string when UserName is null");
    }

    [Fact]
    public void ToString_ParameterizedConstructor_ReturnsUserName()
    {
        const string userName = "john.doe@example.com";
        var user = new ApplicationUser(userName);

        var result = user.ToString();

        result.Should().Be(userName, "ToString should return the user name");
    }

    [Fact]
    public void UserNamesWithDifferentCasing_ProduceSameNormalizedUserName()
    {
        var user1 = new ApplicationUser("john.doe@example.com");
        var user2 = new ApplicationUser("JOHN.DOE@EXAMPLE.COM");

        user1.NormalizedUserName.Should().Be("JOHN.DOE@EXAMPLE.COM", "first user should normalize correctly");
        user2.NormalizedUserName.Should().Be("JOHN.DOE@EXAMPLE.COM", "second user should normalize correctly");
        user1.NormalizedUserName.Should().Be(user2.NormalizedUserName, "both users should have the same normalized user name");

        Action act = () => _ = new ApplicationUser("john..doe@example.com");
        act.Should().Throw<ArgumentException>().WithParameterName("userName");
    }
}
