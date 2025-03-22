namespace HeroBoxAI.WebApi.Endpoints;

/// <summary>
/// Standard API response wrapper
/// </summary>
/// <typeparam name="T">Type of data being returned</typeparam>
/// <param name="Success">Whether the operation was successful</param>
/// <param name="Message">Human-readable message about the operation</param>
/// <param name="Data">The data being returned</param>
public record ApiResponse<T>(bool Success, string Message, T Data); 