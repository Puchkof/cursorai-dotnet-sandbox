using HeroBoxAI.Domain.Entities;
using HeroBoxAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HeroBoxAI.WebApi.Endpoints;

public static class ItemEndpoints
{
    public static void MapItemEndpoints(this WebApplication app)
    {
        // TODO: Move to GetAllItemsQuery in Application layer
        app.MapGet(ApiRoutes.Items.Base, async (HeroBoxDbContext db) =>
            await db.Items
                .Select(i => new 
                {
                    i.Id,
                    i.Name,
                    i.Description,
                    i.Type,
                    i.Rarity,
                    i.RequiredLevel,
                    i.Quantity,
                    i.IsEquipped,
                    i.AcquiredAt,
                    HeroId = i.HeroId,
                    HeroName = i.Hero.Name
                })
                .ToListAsync())
            .WithName("GetAllItems")
            .WithOpenApi();

        // TODO: Move to GetItemByIdQuery in Application layer
        app.MapGet(ApiRoutes.Items.ById, async (Guid id, HeroBoxDbContext db) =>
            await db.Items.FindAsync(id) is Item item
                ? Results.Ok(new ApiResponse<Item>(true, "Item found", item))
                : Results.NotFound(new ApiResponse<string>(false, "Item not found", string.Empty)))
            .WithName("GetItemById")
            .WithOpenApi();

        // TODO: Move to GetHeroItemsQuery in Application layer
        app.MapGet(ApiRoutes.Heroes.Items, async (Guid id, HeroBoxDbContext db) =>
            await db.Items.Where(i => i.HeroId == id).ToListAsync() is List<Item> items
                ? Results.Ok(new ApiResponse<List<Item>>(true, "Items found", items))
                : Results.NotFound(new ApiResponse<string>(false, "No items found for this hero", string.Empty)))
            .WithName("GetHeroItems")
            .WithOpenApi();

        // TODO: Move to CreateItemCommand in Application layer
        app.MapPost(ApiRoutes.Items.Base, async (Item item, HeroBoxDbContext db) =>
        {
            item.Id = Guid.NewGuid();
            item.AcquiredAt = DateTime.UtcNow;
            item.Quantity = Math.Max(1, item.Quantity);
            
            db.Items.Add(item);
            await db.SaveChangesAsync();
            
            return Results.Created($"{ApiRoutes.Items.Base}/{item.Id}", 
                new ApiResponse<Item>(true, "Item created successfully", item));
        })
        .WithName("CreateItem")
        .WithOpenApi();

        // TODO: Move to UpdateItemCommand in Application layer
        app.MapPut(ApiRoutes.Items.ById, async (Guid id, Item updatedItem, HeroBoxDbContext db) =>
        {
            var item = await db.Items.FindAsync(id);
            
            if (item == null)
                return Results.NotFound(new ApiResponse<string>(false, "Item not found", string.Empty));
            
            item.Name = updatedItem.Name;
            item.Description = updatedItem.Description;
            item.Type = updatedItem.Type;
            item.Rarity = updatedItem.Rarity;
            item.RequiredLevel = updatedItem.RequiredLevel;
            item.Quantity = updatedItem.Quantity;
            item.IsEquipped = updatedItem.IsEquipped;
            
            await db.SaveChangesAsync();
            
            return Results.Ok(new ApiResponse<Item>(true, "Item updated successfully", item));
        })
        .WithName("UpdateItem")
        .WithOpenApi();

        // TODO: Move to DeleteItemCommand in Application layer
        app.MapDelete(ApiRoutes.Items.ById, async (Guid id, HeroBoxDbContext db) =>
        {
            var item = await db.Items.FindAsync(id);
            
            if (item == null)
                return Results.NotFound(new ApiResponse<string>(false, "Item not found", string.Empty));
            
            db.Items.Remove(item);
            await db.SaveChangesAsync();
            
            return Results.Ok(new ApiResponse<string>(true, "Item deleted successfully", string.Empty));
        })
        .WithName("DeleteItem")
        .WithOpenApi();
    }
} 