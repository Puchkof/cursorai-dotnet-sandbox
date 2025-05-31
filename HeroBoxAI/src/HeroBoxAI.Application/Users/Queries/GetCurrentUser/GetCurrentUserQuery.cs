using System;
using MediatR;

namespace HeroBoxAI.Application.Users.Queries.GetCurrentUser;

public record GetCurrentUserQuery(Guid UserId) : IRequest<UserDto>; 