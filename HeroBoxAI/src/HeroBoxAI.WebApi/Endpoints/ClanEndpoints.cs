using HeroBoxAI.Application.Clans;
using HeroBoxAI.Application.Clans.Commands.CreateClan;
using HeroBoxAI.Application.Clans.Commands.UpdateClan;
using HeroBoxAI.Application.Clans.Commands.DeleteClan;
using HeroBoxAI.Application.Clans.Queries.GetAllClans;
using HeroBoxAI.Application.Clans.Queries.GetClanById;
using HeroBoxAI.Application.Clans.Queries.GetClanMembers;
using HeroBoxAI.Application.Common.Interfaces;
using MediatR;

namespace HeroBoxAI.WebApi.Endpoints;

public static class ClanEndpoints
{
    public static void MapClanEndpoints(this WebApplication app)
    {
        // Public endpoints - no authentication required
        app.MapGet(ApiRoutes.Clans.Base, async (IMediator mediator) =>
        {
            var clans = await mediator.Send(new GetAllClansQuery());
            return Results.Ok(clans);
        })
        .WithName("GetAllClans")
        .WithOpenApi();

        app.MapGet(ApiRoutes.Clans.ById, async (Guid id, IMediator mediator) =>
        {
            var clan = await mediator.Send(new GetClanByIdQuery(id));
            return clan != null 
                ? Results.Ok(clan)
                : Results.NotFound();
        })
        .WithName("GetClanById")
        .WithOpenApi();

        app.MapGet(ApiRoutes.Clans.Members, async (Guid id, IMediator mediator) =>
        {
            var members = await mediator.Send(new GetClanMembersQuery(id));
            return Results.Ok(members);
        })
        .WithName("GetClanMembers")
        .WithOpenApi();

        // Protected endpoints - require authentication
        app.MapPost(ApiRoutes.Clans.Base, async (CreateClanCommand command, IUserContext userContext, IMediator mediator) =>
        {
            var userId = userContext.GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Results.Unauthorized();
            }

            // Set the FounderId from the current user context
            var createCommand = command with { FounderId = userId.Value };
            var clan = await mediator.Send(createCommand);
            return clan != null
                ? Results.Created($"{ApiRoutes.Clans.Base}/{clan.Id}", clan)
                : Results.BadRequest("Unable to create clan. Founder not found.");
        })
        .RequireAuthorization()
        .WithName("CreateClan")
        .WithOpenApi();

        app.MapPut(ApiRoutes.Clans.ById, async (Guid id, UpdateClanCommand command, IUserContext userContext, IMediator mediator) =>
        {
            var userId = userContext.GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Results.Unauthorized();
            }

            // Ensure the ID from the route matches the command and set CurrentUserId
            var updateCommand = command with { Id = id, CurrentUserId = userId.Value };
            var clan = await mediator.Send(updateCommand);
            return clan != null
                ? Results.Ok(clan)
                : Results.NotFound("Clan not found.");
        })
        .RequireAuthorization()
        .WithName("UpdateClan")
        .WithOpenApi();

        app.MapDelete(ApiRoutes.Clans.ById, async (Guid id, IUserContext userContext, IMediator mediator) =>
        {
            var userId = userContext.GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Results.Unauthorized();
            }

            var success = await mediator.Send(new DeleteClanCommand(id, userId.Value));
            return success
                ? Results.NoContent()
                : Results.NotFound("Clan not found.");
        })
        .RequireAuthorization()
        .WithName("DeleteClan")
        .WithOpenApi();
    }
} 