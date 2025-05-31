using HeroBoxAI.Application.Common.Interfaces;
using HeroBoxAI.Application.Users.Commands.SignIn;
using HeroBoxAI.Application.Users.Commands.SignUp;
using HeroBoxAI.Application.Users.Commands.UpdateCurrentUser;
using HeroBoxAI.Application.Users.Queries.GetCurrentUser;
using MediatR;

namespace HeroBoxAI.WebApi.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        // Authentication endpoints
        app.MapPost(ApiRoutes.Auth.SignUp, async (SignUpCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Ok(result);
        })
        .WithName("SignUp")
        .WithOpenApi();

        app.MapPost(ApiRoutes.Auth.SignIn, async (SignInCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Ok(result);
        })
        .WithName("SignIn")
        .WithOpenApi();

        // Current user endpoints (require authentication)
        app.MapGet(ApiRoutes.Users.Me, async (IUserContext userContext, IMediator mediator) =>
        {
            var userId = userContext.GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Results.Unauthorized();
            }

            var currentUser = await mediator.Send(new GetCurrentUserQuery(userId.Value));
            return Results.Ok(currentUser);
        })
        .RequireAuthorization()
        .WithName("GetCurrentUser")
        .WithOpenApi();

        app.MapPut(ApiRoutes.Users.Me, async (UpdateCurrentUserCommand command, IUserContext userContext, IMediator mediator) =>
        {
            var userId = userContext.GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Results.Unauthorized();
            }

            var updateCommand = command with { UserId = userId.Value };
            var result = await mediator.Send(updateCommand);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("UpdateCurrentUser")
        .WithOpenApi();
    }
} 