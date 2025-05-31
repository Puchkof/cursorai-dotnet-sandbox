using System;

namespace HeroBoxAI.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public ValidationException(string message) 
        : base(message)
    {
    }

    public ValidationException(string field, string message) 
        : base($"{field}: {message}")
    {
        Field = field;
    }

    public string? Field { get; }
} 