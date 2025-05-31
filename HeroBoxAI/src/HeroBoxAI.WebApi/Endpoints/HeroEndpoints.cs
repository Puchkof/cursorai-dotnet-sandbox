using HeroBoxAI.Domain.Entities;
using HeroBoxAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HeroBoxAI.WebApi.Endpoints;

public static class HeroEndpoints
{
    public static void MapHeroEndpoints(this WebApplication app)
    {
        app.MapGet(ApiRoutes.Heroes.Base, async (HeroBoxDbContext db) =>
            await db.Heroes
                .Select(h => new 
                {
                    h.Id,
                    h.Name,
                    h.Class,
                    h.Level,
                    h.Experience,
                    h.CreatedAt,
                    UserId = h.UserId,
                    UserName = h.User.Username,
                    ItemCount = h.Items.Count
                })
                .ToListAsync())
            .WithName("GetAllHeroes")
            .WithOpenApi();

        app.MapGet(ApiRoutes.Heroes.ById, async (Guid id, HeroBoxDbContext db) =>
            await db.Heroes.FindAsync(id) is Hero hero
                ? Results.Ok(new ApiResponse<Hero>(true, "Hero found", hero))
                : Results.NotFound(new ApiResponse<string>(false, "Hero not found", string.Empty)))
            .WithName("GetHeroById")
            .WithOpenApi();

        app.MapPost(ApiRoutes.Heroes.Base, async (Hero hero, HeroBoxDbContext db) =>
        {
            hero.Id = Guid.NewGuid();
            hero.CreatedAt = DateTime.UtcNow;
            hero.Level = 1;
            hero.Experience = 0;
            
            db.Heroes.Add(hero);
            await db.SaveChangesAsync();
            
            return Results.Created($"{ApiRoutes.Heroes.Base}/{hero.Id}", 
                new ApiResponse<Hero>(true, "Hero created successfully", hero));
        })
        .WithName("CreateHero")
        .WithOpenApi();

        app.MapPut(ApiRoutes.Heroes.ById, async (Guid id, Hero updatedHero, HeroBoxDbContext db) =>
        {
            var hero = await db.Heroes.FindAsync(id);
            
            if (hero == null)
                return Results.NotFound(new ApiResponse<string>(false, "Hero not found", string.Empty));
            
            hero.Name = updatedHero.Name;
            hero.Class = updatedHero.Class;
            hero.Level = updatedHero.Level;
            hero.Experience = updatedHero.Experience;
            
            await db.SaveChangesAsync();
            
            return Results.Ok(new ApiResponse<Hero>(true, "Hero updated successfully", hero));
        })
        .WithName("UpdateHero")
        .WithOpenApi();

        app.MapDelete(ApiRoutes.Heroes.ById, async (Guid id, HeroBoxDbContext db) =>
        {
            var hero = await db.Heroes.FindAsync(id);
            
            if (hero == null)
                return Results.NotFound(new ApiResponse<string>(false, "Hero not found", string.Empty));
            
            db.Heroes.Remove(hero);
            await db.SaveChangesAsync();
            
            return Results.Ok(new ApiResponse<string>(true, "Hero deleted successfully", string.Empty));
        })
        .WithName("DeleteHero")
        .WithOpenApi();
    }
}