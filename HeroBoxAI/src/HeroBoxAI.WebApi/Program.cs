using HeroBoxAI.Infrastructure;
using HeroBoxAI.Application;
using HeroBoxAI.WebApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add application services (including MediatR)
builder.Services.AddApplicationServices();

// Add infrastructure services (including DbContext)
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Apply database migrations regardless of environment
await app.ApplyMigrationsAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Register API endpoints by domain
app.MapUserEndpoints();
app.MapItemEndpoints();
app.MapClanEndpoints();

// Uncomment when ready to use
// app.MapHeroEndpoints();

app.Run();

// Make the Program class public so it can be accessed by tests
public partial class Program { }
