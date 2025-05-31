using System.Threading;
using System.Threading.Tasks;
using HeroBoxAI.Application.Common.Exceptions;
using HeroBoxAI.Domain.Repositories;
using MediatR;

namespace HeroBoxAI.Application.Users.Commands.UpdateCurrentUser;

public class UpdateCurrentUserCommandHandler : IRequestHandler<UpdateCurrentUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;

    public UpdateCurrentUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> Handle(UpdateCurrentUserCommand request, CancellationToken cancellationToken)
    {
        // Validate input parameters
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new ValidationException("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ValidationException("Email is required.");
        }

        // Validate email format
        if (!IsValidEmail(request.Email))
        {
            throw new ValidationException("Invalid email format.");
        }

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user == null)
        {
            throw new UserNotFoundException(request.UserId.ToString());
        }

        // Check if username is already taken by another user
        if (user.Username != request.Username && 
            await _userRepository.ExistsByUsernameAsync(request.Username, cancellationToken))
        {
            throw new UserAlreadyExistsException("username", request.Username);
        }

        // Check if email is already taken by another user
        if (user.Email != request.Email && 
            await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            throw new UserAlreadyExistsException("email", request.Email);
        }

        // Update user properties
        user.Username = request.Username;
        user.Email = request.Email;

        await _userRepository.SaveChangesAsync(cancellationToken);

        return new UserDto
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
        };
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
} 