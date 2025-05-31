using System;
using HeroBoxAI.Domain.Entities;

namespace HeroBoxAI.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    Guid? GetUserIdFromToken(string token);
} 