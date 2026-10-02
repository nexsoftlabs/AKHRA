using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using MoviePlatform.Application.Identity;
using MoviePlatform.Infrastructure.Identity;
using Xunit;

namespace MoviePlatform.Tests.Integration;

[Trait("Category", "Integration")]
public class AuthIntegrationTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _postgres;
    private readonly PostgresWebApplicationFactory _factory;

    public AuthIntegrationTests(PostgresFixture postgres)
    {
        _postgres = postgres;
        _factory = new PostgresWebApplicationFactory(postgres);
    }
    private const string StrongPassword = "TestPass123!@#";

    [SkippableFact]
    public async Task Register_Login_And_GetProfile_Works()
    {
        Skip.IfNot(_postgres.IsAvailable, "Docker is required for integration tests.");
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        var email = $"user_{Guid.NewGuid():N}@example.com";

        var register = await client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(email, StrongPassword, "Test User"));
        Assert.Equal(HttpStatusCode.OK, register.StatusCode);
        var profile = await register.Content.ReadFromJsonAsync<UserProfileDto>();
        Assert.NotNull(profile);
        Assert.Equal(email, profile!.Email);

        var me = await client.GetAsync("/api/v1/users/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);

        await client.PostAsync("/api/v1/auth/logout", null);
        var meAfterLogout = await client.GetAsync("/api/v1/users/me");
        Assert.Equal(HttpStatusCode.Unauthorized, meAfterLogout.StatusCode);

        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, StrongPassword));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [SkippableFact]
    public async Task Login_WithWrongPassword_ReturnsBadRequest()
    {
        Skip.IfNot(_postgres.IsAvailable, "Docker is required for integration tests.");
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        var email = $"user_{Guid.NewGuid():N}@example.com";
        await client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(email, StrongPassword, null));

        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, "WrongPass123!@#"));
        Assert.Equal(HttpStatusCode.BadRequest, login.StatusCode);
    }

    [SkippableFact]
    public async Task Otp_CannotBeReused()
    {
        Skip.IfNot(_postgres.IsAvailable, "Docker is required for integration tests.");
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        var phone = "9876543210";

        var request = await client.PostAsJsonAsync("/api/v1/auth/otp/request", new OtpRequest(phone));
        Assert.Equal(HttpStatusCode.OK, request.StatusCode);

        var message = DevSmsSender.GetLastMessageFor("+919876543210");
        Assert.NotNull(message);
        var code = Regex.Match(message!, @"\d{6}").Value;
        Assert.Equal(6, code.Length);

        var verify1 = await client.PostAsJsonAsync("/api/v1/auth/otp/verify", new OtpVerifyRequest(phone, code));
        Assert.Equal(HttpStatusCode.OK, verify1.StatusCode);

        var verify2 = await client.PostAsJsonAsync("/api/v1/auth/otp/verify", new OtpVerifyRequest(phone, code));
        Assert.Equal(HttpStatusCode.BadRequest, verify2.StatusCode);
    }

    [SkippableFact]
    public async Task UsersMe_WithoutAuth_ReturnsUnauthorized()
    {
        Skip.IfNot(_postgres.IsAvailable, "Docker is required for integration tests.");
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/users/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
