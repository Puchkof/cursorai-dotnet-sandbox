using System;

namespace HeroBoxAI.Application.Common.Interfaces;

public interface IUserContext
{
    Guid? GetCurrentUserId();
    bool IsAuthenticated();
} 