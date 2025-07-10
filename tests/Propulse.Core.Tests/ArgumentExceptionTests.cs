using AwesomeAssertions;

namespace Propulse.Core.Tests;

public class ArgumentExceptionTests
{
    #region ThrowIfNotValidRoleName

    [Theory]
    [InlineData("Admin")]
    [InlineData("User")]
    [InlineData("Manager")]
    [InlineData("SuperAdmin")]
    [InlineData("ContentEditor")]
    [InlineData("SystemAdministrator")]
    [InlineData("ABC")] // Minimum length (3 characters)
    [InlineData("ADMIN")] // All uppercase is valid per regex
    [InlineData("USER")] // All uppercase is valid per regex
    public void ThrowIfNotValidRoleName_ValidRoleName_DoesNotThrow(string testValue)
    {
        // Arrange & Act
        Action act = () => ArgumentExceptionExtensions.ThrowIfNotValidRoleName(testValue);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("admin")] // Starts with lowercase
    [InlineData("Ad")] // Too short (2 characters)
    [InlineData("A")] // Too short (1 character)
    [InlineData("")] // Empty string
    [InlineData("Admin123")] // Contains numbers
    [InlineData("Admin-User")] // Contains hyphen
    [InlineData("Admin_User")] // Contains underscore
    [InlineData("Admin User")] // Contains space
    [InlineData("Admin@User")] // Contains special character
    [InlineData("Ädmin")] // Contains non-ASCII character
    public void ThrowIfNotValidRoleName_InvalidRoleName_ThrowsArgumentException(string testValue)
    {
        // Arrange & Act
        Action act = () => ArgumentExceptionExtensions.ThrowIfNotValidRoleName(testValue);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName(nameof(testValue));
    }

    [Fact]
    public void ThrowIfNotValidRoleName_NullArgument_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => ArgumentExceptionExtensions.ThrowIfNotValidRoleName(null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region ThrowIfNotValidUserName

    [Theory]
    [InlineData("john.doe@example.com")]
    [InlineData("jane-doe@sub.example.com")]
    [InlineData("user_name@xn--d1acufc.xn--p1ai")] // punycode domain
    [InlineData("a.b-c_d@a-b.c-d.com")]
    [InlineData("a@b.co")] // minimal valid segments
    [InlineData("a.b@a.b.c.d.e.f")] // 6 segments
    [InlineData("user@ex-ample.com")]
    [InlineData("user@ex.ample.com")]
    [InlineData("user@xn--bcher-kva.ch")] // IDN
    [InlineData("user@bücher.ch")] // Unicode domain
    [InlineData("user@a.b")] // minimal valid domain
    [InlineData("user@a.b.c.d.e.f")] // maximal valid domain
    [InlineData(".johndoe@example.com")] // leading dot is allowed
    [InlineData("johndoe.@example.com")] // trailing dot is allowed
    [InlineData("john.doe@example.c")] // single char segment is valid
    public void ThrowIfNotValidUserName_ValidUserName_DoesNotThrow(string testValue)
    {
        // Arrange & Act
        Action act = () => ArgumentExceptionExtensions.ThrowIfNotValidUserName(testValue);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("john..doe@example.com")] // consecutive dots in username
    [InlineData("john#doe@example.com")] // invalid char in username
    [InlineData("john.doe@.example.com")] // empty segment
    [InlineData("john.doe@example..com")] // empty segment
    [InlineData("john.doe@example")] // only one segment
    [InlineData("john.doe@example.com.")] // trailing dot creates empty segment
    [InlineData("john.doe@.com")] // leading dot creates empty segment
    [InlineData("john.doe@com")] // only one segment
    [InlineData("john.doe@example.com.org.net.edu.gov.uk")] // 7 segments (too many)
    [InlineData("john.doe@")] // missing domain
    [InlineData("@example.com")] // missing username
    [InlineData("john.doeexample.com")] // missing @
    [InlineData("john.doe@@example.com")] // double @
    [InlineData("john.doe@xn--")] // invalid punycode
    [InlineData("john.doe@xn--.com")] // invalid punycode segment
    [InlineData("john.doe@bücher..ch")] // empty segment with unicode
    public void ThrowIfNotValidUserName_InValidUserName_ThrowsArgumentException(string testValue)
    {
        // Arrange & Act
        Action act = () => ArgumentExceptionExtensions.ThrowIfNotValidUserName(testValue);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName(nameof(testValue));
    }

    [Fact]
    public void ThrowIfNotValidUserName_NullArgument_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => ArgumentExceptionExtensions.ThrowIfNotValidUserName(null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion
}