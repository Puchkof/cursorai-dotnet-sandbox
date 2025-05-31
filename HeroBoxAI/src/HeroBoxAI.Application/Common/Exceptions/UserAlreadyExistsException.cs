using System;

namespace HeroBoxAI.Application.Common.Exceptions;

public class UserAlreadyExistsException : Exception
{
    public UserAlreadyExistsException(string field, string value) 
        : base($"User with {field} '{value}' already exists.")
    {
        Field = field;
        Value = value;
    }

    public string Field { get; }
    public string Value { get; }
} 