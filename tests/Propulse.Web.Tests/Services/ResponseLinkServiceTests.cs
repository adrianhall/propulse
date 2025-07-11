using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Propulse.Web.Services;

namespace Propulse.Web.Tests.Services;

/// <summary>
/// Unit tests for <see cref="ResponseLinkService"/>.
/// </summary>
public class ResponseLinkServiceTests
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LinkGenerator _linkGenerator;
    private readonly ILogger<ResponseLinkService> _logger;
    private readonly HttpContext _httpContext;
    private readonly ResponseLinkService _service;

    public ResponseLinkServiceTests()
    {
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _linkGenerator = Substitute.For<LinkGenerator>();
        _logger = Substitute.For<ILogger<ResponseLinkService>>();
        _httpContext = Substitute.For<HttpContext>();

        _httpContextAccessor.HttpContext.Returns(_httpContext);

        _service = new ResponseLinkService(_httpContextAccessor, _linkGenerator, _logger);
    }

    private void SetLinkGeneratorAddress(string? link)
    {
        _linkGenerator.GetUriByAddress<RouteValuesAddress>(
            _httpContext,
            Arg.Any<RouteValuesAddress>(),
            Arg.Any<RouteValueDictionary>(),
            Arg.Any<RouteValueDictionary>(),
            Arg.Any<string>(),
            Arg.Any<HostString?>(),
            Arg.Any<PathString?>(),
            Arg.Any<FragmentString>(),
            Arg.Any<LinkOptions>())
            .Returns(link);
    }

    #region CreateResponseLink Tests

    [Fact]
    public void CreateResponseLink_WithValidParameters_ReturnsGeneratedLink()
    {
        // Arrange
        const string area = "TestArea";
        const string controller = "TestController";
        const string action = "TestAction";
        const string code = "test-code";
        const string expectedLink = "https://example.com/TestArea/TestController/TestAction?code=test-code";
        SetLinkGeneratorAddress(expectedLink);

        // Act
        var result = _service.CreateResponseLink(area, controller, action, code);

        // Assert
        result.Should().Be(expectedLink);
        _linkGenerator.Received(1).GetUriByAddress<RouteValuesAddress>(
            _httpContext,
            Arg.Any<RouteValuesAddress>(),
            Arg.Any<RouteValueDictionary>(),
            Arg.Any<RouteValueDictionary>(),
            Arg.Any<string>(),
            Arg.Any<HostString?>(),
            Arg.Any<PathString?>(),
            Arg.Any<FragmentString>(),
            Arg.Any<LinkOptions>());
    }

    [Fact]
    public void CreateResponseLink_WithEmptyArea_ReturnsGeneratedLink()
    {
        // Arrange
        const string area = "";
        const string controller = "TestController";
        const string action = "TestAction";
        const string code = "test-code";
        const string expectedLink = "https://example.com/TestController/TestAction?code=test-code";
        SetLinkGeneratorAddress(expectedLink);

        // Act
        var result = _service.CreateResponseLink(area, controller, action, code);

        // Assert
        result.Should().Be(expectedLink);
    }

    [Fact]
    public void CreateResponseLink_WhenLinkGeneratorReturnsNull_ThrowsInvalidOperationException()
    {
        // Arrange
        const string area = "TestArea";
        const string controller = "TestController";
        const string action = "TestAction";
        const string code = "test-code";
        SetLinkGeneratorAddress(null);

        // Act & Assert
        Action act = () => _service.CreateResponseLink(area, controller, action, code);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CreateResponseLink_WhenLinkGeneratorReturnsEmptyString_ThrowsInvalidOperationException()
    {
        // Arrange
        const string area = "TestArea";
        const string controller = "TestController";
        const string action = "TestAction";
        const string code = "test-code";
        SetLinkGeneratorAddress(string.Empty);

        // Act & Assert
        Action act = () => _service.CreateResponseLink(area, controller, action, code);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CreateResponseLink_WhenHttpContextIsNull_ThrowsInvalidOperationException()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);
        
        const string area = "TestArea";
        const string controller = "TestController";
        const string action = "TestAction";
        const string code = "test-code";

        // Act & Assert
        Action act = () => _service.CreateResponseLink(area, controller, action, code);
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region EncodeResponseCode Tests

    [Fact]
    public void EncodeResponseCode_WithValidParameters_ReturnsEncodedString()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string token = "test-token";

        // Act
        var result = _service.EncodeResponseCode(id, token);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().NotContain("="); // Base64Url encoding should not contain padding
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void EncodeResponseCode_WithNullOrEmptyToken_ThrowsArgumentException(string? token)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act & Assert
        Action act = () => _service.EncodeResponseCode(id, token!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EncodeResponseCode_WithDifferentTokens_ProducesDifferentResults()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string token1 = "token1";
        const string token2 = "token2";

        // Act
        var result1 = _service.EncodeResponseCode(id, token1);
        var result2 = _service.EncodeResponseCode(id, token2);

        // Assert
        result1.Should().NotBe(result2);
    }

    [Fact]
    public void EncodeResponseCode_WithDifferentIds_ProducesDifferentResults()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        const string token = "same-token";

        // Act
        var result1 = _service.EncodeResponseCode(id1, token);
        var result2 = _service.EncodeResponseCode(id2, token);

        // Assert
        result1.Should().NotBe(result2);
    }

    [Fact]
    public void EncodeResponseCode_WithSameParameters_ProducesSameResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string token = "test-token";

        // Act
        var result1 = _service.EncodeResponseCode(id, token);
        var result2 = _service.EncodeResponseCode(id, token);

        // Assert
        result1.Should().Be(result2);
    }

    [Fact]
    public void EncodeResponseCode_WithUnicodeToken_HandlesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string token = "тест-токен-🚀";

        // Act
        var result = _service.EncodeResponseCode(id, token);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region DecodeResponseCode Tests

    [Fact]
    public void DecodeResponseCode_WithValidCode_ReturnsOriginalIdAndToken()
    {
        // Arrange
        var originalId = Guid.NewGuid();
        const string originalToken = "test-token";
        var code = _service.EncodeResponseCode(originalId, originalToken);

        // Act
        var (decodedId, decodedToken) = _service.DecodeResponseCode(code);

        // Assert
        decodedId.Should().Be(originalId);
        decodedToken.Should().Be(originalToken);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void DecodeResponseCode_WithNullOrEmptyCode_ThrowsArgumentException(string? code)
    {
        // Act & Assert
        Action act = () => _service.DecodeResponseCode(code!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DecodeResponseCode_WithInvalidBase64_ThrowsFormatException()
    {
        // Arrange
        const string invalidCode = "invalid-base64!@#$%";

        // Act & Assert
        Action act = () => _service.DecodeResponseCode(invalidCode);
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void DecodeResponseCode_WithTooShortCode_ThrowsFormatException()
    {
        // Arrange - Create a code that's too short (less than 17 bytes when decoded)
        var shortBytes = new byte[16]; // Only 16 bytes, need at least 17
        var shortCode = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(shortBytes);

        // Act & Assert
        Action act = () => _service.DecodeResponseCode(shortCode);
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void DecodeResponseCode_WithMinimumValidLength_ReturnsCorrectly()
    {
        // Arrange - Create exactly 17 bytes (16 for GUID + 1 for minimal token)
        var id = Guid.NewGuid();
        const string token = "a"; // Single character token
        var code = _service.EncodeResponseCode(id, token);

        // Act
        var (decodedId, decodedToken) = _service.DecodeResponseCode(code);

        // Assert
        decodedId.Should().Be(id);
        decodedToken.Should().Be(token);
    }

    [Fact]
    public void DecodeResponseCode_WithUnicodeToken_ReturnsCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string token = "тест-токен-🚀";
        var code = _service.EncodeResponseCode(id, token);

        // Act
        var (decodedId, decodedToken) = _service.DecodeResponseCode(code);

        // Assert
        decodedId.Should().Be(id);
        decodedToken.Should().Be(token);
    }

    [Fact]
    public void DecodeResponseCode_WithLongToken_ReturnsCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var token = new string('a', 1000); // Very long token
        var code = _service.EncodeResponseCode(id, token);

        // Act
        var (decodedId, decodedToken) = _service.DecodeResponseCode(code);

        // Assert
        decodedId.Should().Be(id);
        decodedToken.Should().Be(token);
    }

    #endregion

    #region Roundtrip Tests

    [Theory]
    [InlineData(null, "simple-token")]
    [InlineData("12345678-1234-5678-9ABC-123456789ABC", "specific-guid")]
    [InlineData(null, "unicode-токен-🎉")]
    [InlineData(null, "special!@#$%^&*()_+-=[]{}|;':\",./<>?")]
    [InlineData(null, "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx")]
    [InlineData(null, "a")]
    [InlineData(null, "    spaces    ")]
    public void IdAndTokenCanRoundtripViaCode_WithVariousInputs(string? sGuid, string token)
    {
        // Arrange
        Guid id = sGuid is null ? Guid.NewGuid() : new Guid(sGuid);

        // Act
        var encodedCode = _service.EncodeResponseCode(id, token);
        var (decodedId, decodedToken) = _service.DecodeResponseCode(encodedCode);

        // Assert
        decodedId.Should().Be(id, $"ID should roundtrip correctly for token: {token}");
        decodedToken.Should().Be(token, $"Token should roundtrip correctly for token: {token}");
    }

    [Fact]
    public void RoundtripTest_MultipleIterations_ConsistentResults()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string token = "consistent-token";

        // Act & Assert - Multiple encode/decode cycles should be consistent
        for (int i = 0; i < 10; i++)
        {
            var code = _service.EncodeResponseCode(id, token);
            var (decodedId, decodedToken) = _service.DecodeResponseCode(code);

            decodedId.Should().Be(id, $"Iteration {i}: ID should be consistent");
            decodedToken.Should().Be(token, $"Iteration {i}: Token should be consistent");
        }
    }

    [Fact]
    public void RoundtripTest_WithBinaryTokenData_HandlesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        // Create a token that contains various binary data represented as string
        var binaryBytes = new byte[] { 0x00, 0x01, 0xFF, 0x7F, 0x80, 0xDE, 0xAD, 0xBE, 0xEF };
        var token = Convert.ToBase64String(binaryBytes);

        // Act
        var code = _service.EncodeResponseCode(id, token);
        var (decodedId, decodedToken) = _service.DecodeResponseCode(code);

        // Assert
        decodedId.Should().Be(id);
        decodedToken.Should().Be(token);
    }

    #endregion
}
