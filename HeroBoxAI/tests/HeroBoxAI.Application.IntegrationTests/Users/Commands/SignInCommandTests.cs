using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using HeroBoxAI.Application.Users;
using HeroBoxAI.Application.Users.Commands.SignIn;
using HeroBoxAI.Application.Users.Commands.SignUp;
using HeroBoxAI.WebApi.Endpoints;
using Xunit;

namespace HeroBoxAI.Application.IntegrationTests.Users.Commands;

public class SignInCommandTests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;
    private readonly ApiTestFixture _fixture;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public SignInCommandTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task Handle_ValidEmailAndPassword_ReturnsAuthResponse()
    {
        // Arrange
        var signUpCommand = new SignUpCommand
        {
            Username = "signinuser",
            Email = "signin@example.com",
            Password = "TestPassword123!"
        };

        // First create a user
        await _client.PostAsJsonAsync(ApiRoutes.Auth.SignUp, signUpCommand);

        var signInCommand = new SignInCommand
        {
            Email = "signin@example.com",
            Username = null,
            Password = "TestPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync(ApiRoutes.Auth.SignIn, signInCommand);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.NotEmpty(result.Token);
        Assert.NotNull(result.User);
        Assert.Equal(signUpCommand.Username, result.User.Username);
        Assert.Equal(signUpCommand.Email, result.User.Email);
    }

    [Fact]
    public async Task Handle_ValidUsernameAndPassword_ReturnsAuthResponse()
    {
        // Arrange
        var signUpCommand = new SignUpCommand
        {
            Username = "signinuser2",
            Email = "signin2@example.com",
            Password = "TestPassword123!"
        };

        // First create a user
        await _client.PostAsJsonAsync(ApiRoutes.Auth.SignUp, signUpCommand);

        var signInCommand = new SignInCommand
        {
            Email = null,
            Username = "signinuser2",
            Password = "TestPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync(ApiRoutes.Auth.SignIn, signInCommand);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.NotEmpty(result.Token);
        Assert.NotNull(result.User);
        Assert.Equal(signUpCommand.Username, result.User.Username);
        Assert.Equal(signUpCommand.Email, result.User.Email);
    }

    [Fact]
    public async Task Handle_InvalidEmail_ReturnsUnauthorized()
    {
        // Arrange
        var signInCommand = new SignInCommand
        {
            Email = "nonexistent@example.com",
            Username = null,
            Password = "TestPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync(ApiRoutes.Auth.SignIn, signInCommand);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Handle_InvalidUsername_ReturnsUnauthorized()
    {
        // Arrange
        var signInCommand = new SignInCommand
        {
            Email = null,
            Username = "nonexistentuser",
            Password = "TestPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync(ApiRoutes.Auth.SignIn, signInCommand);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Handle_InvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var signUpCommand = new SignUpCommand
        {
            Username = "signinuser3",
            Email = "signin3@example.com",
            Password = "TestPassword123!"
        };

        // First create a user
        await _client.PostAsJsonAsync(ApiRoutes.Auth.SignUp, signUpCommand);

        var signInCommand = new SignInCommand
        {
            Email = "signin3@example.com",
            Username = null,
            Password = "WrongPassword!"
        };

        // Act
        var response = await _client.PostAsJsonAsync(ApiRoutes.Auth.SignIn, signInCommand);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Handle_BothEmailAndUsernameProvided_ReturnsBadRequest()
    {
        // Arrange
        var signInCommand = new SignInCommand
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "TestPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync(ApiRoutes.Auth.SignIn, signInCommand);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Handle_NeitherEmailNorUsernameProvided_ReturnsBadRequest()
    {
        // Arrange
        var signInCommand = new SignInCommand
        {
            Email = null,
            Username = null,
            Password = "TestPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync(ApiRoutes.Auth.SignIn, signInCommand);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Handle_EmptyPassword_ReturnsBadRequest()
    {
        // Arrange
        var signInCommand = new SignInCommand
        {
            Email = "test@example.com",
            Username = null,
            Password = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync(ApiRoutes.Auth.SignIn, signInCommand);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
} 