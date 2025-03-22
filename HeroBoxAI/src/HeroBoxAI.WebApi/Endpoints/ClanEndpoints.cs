using HeroBoxAI.Application.Clans;
using HeroBoxAI.Application.Clans.Commands.CreateClan;
using HeroBoxAI.Application.Clans.Queries.GetAllClans;
using HeroBoxAI.Application.Clans.Queries.GetClanById;
using HeroBoxAI.Domain.Entities;
using MediatR;

namespace HeroBoxAI.WebApi.Endpoints;

public static class ClanEndpoints
{
    public static void MapClanEndpoints(this WebApplication app)
    {
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
            // TODO: Replace with MediatR query when implemented
            Results.Ok(new ApiResponse<string>(true, "Endpoint will be updated to use MediatR", "Not implemented yet")))
            .WithName("GetClanMembers")
            .WithOpenApi();

        app.MapPost(ApiRoutes.Clans.Base, async (CreateClanCommand command, IMediator mediator) =>
        {
            var clan = await mediator.Send(command);
            return clan != null
                ? Results.Created($"{ApiRoutes.Clans.Base}/{clan.Id}", clan)
                : Results.BadRequest("Unable to create clan. Founder not found.");
        })
        .WithName("CreateClan")
        .WithOpenApi();

        app.MapPut(ApiRoutes.Clans.ById, async (Guid id, Clan updatedClan, IMediator mediator) =>
            // TODO: Replace with MediatR command when implemented
            Results.Ok(new ApiResponse<string>(true, "Endpoint will be updated to use MediatR", "Not implemented yet")))
            .WithName("UpdateClan")
            .WithOpenApi();

        app.MapDelete(ApiRoutes.Clans.ById, async (Guid id, IMediator mediator) =>
            // TODO: Replace with MediatR command when implemented
            Results.Ok(new ApiResponse<string>(true, "Endpoint will be updated to use MediatR", "Not implemented yet")))
            .WithName("DeleteClan")
            .WithOpenApi();
    }
} 