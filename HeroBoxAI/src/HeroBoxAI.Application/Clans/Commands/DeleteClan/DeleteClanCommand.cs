using System;
using MediatR;

namespace HeroBoxAI.Application.Clans.Commands.DeleteClan;

public record DeleteClanCommand(Guid Id, Guid CurrentUserId) : IRequest<bool>; 