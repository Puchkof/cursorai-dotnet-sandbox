using System;

namespace HeroBoxAI.Application.Common.Exceptions;

public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() 
        : base("Invalid email/username or password.")
    {
    }
} 