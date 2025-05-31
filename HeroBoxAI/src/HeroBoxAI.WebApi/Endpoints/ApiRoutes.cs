namespace HeroBoxAI.WebApi.Endpoints;

/// <summary>
/// Centralized API route definitions
/// </summary>
public static class ApiRoutes
{
    public const string ApiPrefix = "/api";
    
    public static class Users
    {
        public const string Base = ApiPrefix + "/users";
        public const string ById = Base + "/{id:guid}";
        public const string Heroes = ById + "/heroes";
    }
    
    public static class Heroes
    {
        public const string Base = ApiPrefix + "/heroes";
        public const string ById = Base + "/{id:guid}";
        public const string Items = ById + "/items";
    }
    
    public static class Items
    {
        public const string Base = ApiPrefix + "/items";
        public const string ById = Base + "/{id:guid}";
    }
    
    public static class Clans
    {
        public const string Base = ApiPrefix + "/clans";
        public const string ById = Base + "/{id:guid}";
        public const string Members = ById + "/members";
    }
} 