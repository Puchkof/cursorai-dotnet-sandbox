using HeroBoxAI.Domain.Entities;
using HeroBoxAI.Domain.Enums;
using HeroBoxAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HeroBoxAI.WebApi.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapGet(ApiRoutes.Users.Base, async (HeroBoxDbContext db) =>
            await db.Users
                .Select(u => new 
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Role,
                    u.Status,
                    u.CreatedAt,
                    ClanId = u.ClanId,
                    ClanName = u.Clan != null ? u.Clan.Name : null
                })
                .ToListAsync())
            .WithName("GetAllUsers")
            .WithOpenApi();

        app.MapGet(ApiRoutes.Users.ById, async (Guid id, HeroBoxDbContext db) =>
            await db.Users.FindAsync(id) is User user
                ? Results.Ok(new ApiResponse<User>(true, "User found", user))
                : Results.NotFound(new ApiResponse<string>(false, "User not found", string.Empty)))
            .WithName("GetUserById")
            .WithOpenApi();

        app.MapGet(ApiRoutes.Users.Heroes, async (Guid id, HeroBoxDbContext db) =>
            await db.Heroes.Where(h => h.UserId == id).ToListAsync() is List<Hero> heroes
                ? Results.Ok(new ApiResponse<List<Hero>>(true, "Heroes found", heroes))
                : Results.NotFound(new ApiResponse<string>(false, "No heroes found for this user", string.Empty)))
            .WithName("GetUserHeroes")
            .WithOpenApi();

        app.MapPost(ApiRoutes.Users.Base, async (User user, HeroBoxDbContext db) =>
        {
            user.Id = Guid.NewGuid();
            user.CreatedAt = DateTime.UtcNow;
            user.Status = UserStatus.Active;
            
            db.Users.Add(user);
            await db.SaveChangesAsync();
            
            return Results.Created($"{ApiRoutes.Users.Base}/{user.Id}", 
                new ApiResponse<User>(true, "User created successfully", user));
        })
        .WithName("CreateUser")
        .WithOpenApi();

        app.MapPut(ApiRoutes.Users.ById, async (Guid id, User updatedUser, HeroBoxDbContext db) =>
        {
            var user = await db.Users.FindAsync(id);
            
            if (user == null)
                return Results.NotFound(new ApiResponse<string>(false, "User not found", string.Empty));
            
            user.Username = updatedUser.Username;
            user.Email = updatedUser.Email;
            user.Role = updatedUser.Role;
            user.Status = updatedUser.Status;
            
            await db.SaveChangesAsync();
            
            return Results.Ok(new ApiResponse<User>(true, "User updated successfully", user));
        })
        .WithName("UpdateUser")
        .WithOpenApi();

        app.MapDelete(ApiRoutes.Users.ById, async (Guid id, HeroBoxDbContext db) =>
        {
            var user = await db.Users.FindAsync(id);
            
            if (user == null)
                return Results.NotFound(new ApiResponse<string>(false, "User not found", string.Empty));
            
            db.Users.Remove(user);
            await db.SaveChangesAsync();
            
            return Results.Ok(new ApiResponse<string>(true, "User deleted successfully", string.Empty));
        })
        .WithName("DeleteUser")
        .WithOpenApi();
    }
} 