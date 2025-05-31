using System.Threading;
using System.Threading.Tasks;
using HeroBoxAI.Application.Common.Exceptions;
using HeroBoxAI.Application.Common.Interfaces;
using HeroBoxAI.Domain.Repositories;
using MediatR;

namespace HeroBoxAI.Application.Users.Commands.SignIn;

public class SignInCommandHandler : IRequestHandler<SignInCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IJwtTokenService _jwtTokenService;

    public SignInCommandHandler(
        IUserRepository userRepository,
        IPasswordHashService passwordHashService,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHashService = passwordHashService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        // Validate input parameters
        if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.Username))
        {
            throw new ValidationException("Either email or username must be provided.");
        }

        if (!string.IsNullOrWhiteSpace(request.Email) && !string.IsNullOrWhiteSpace(request.Username))
        {
            throw new ValidationException("Provide either email or username, not both.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("Password is required.");
        }

        // Try to find user by email or username
        var user = !string.IsNullOrWhiteSpace(request.Email)
            ? await _userRepository.GetByEmailAsync(request.Email, cancellationToken)
            : await _userRepository.GetByUsernameAsync(request.Username!, cancellationToken);

        if (user == null)
        {
            throw new InvalidCredentialsException();
        }

        // Verify password
        if (!_passwordHashService.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        // Generate JWT token
        var token = _jwtTokenService.GenerateToken(user);

        // Return response
        return new AuthResponseDto
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                ClanId = user.ClanId,
                ClanName = user.Clan?.Name,
                HeroCount = user.Heroes?.Count ?? 0
            }
        };
    }
} 