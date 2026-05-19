using Domain.Products.Repositories;
using MediatR;
using Read.Api.Infrastructure.Persistence;
using Read.Api.Infrastructure.Repositories;
using System.Reflection;

MongoConfiguration.RegisterClassMaps();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});
builder.Services.AddScoped<IProductReadRepository, ProductRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.MapGet("/api/{id:guid}", async (
    Guid id,
    IMediator mediator,
    CancellationToken cancellationToken) =>
{
    var result = await mediator.Send(new Read.Api.Application.Products.Get.Query(id), cancellationToken);
    return result is null ? Results.NotFound() : Results.Ok(result);
});
app.MapGet("/api", async (
    IMediator mediator,
    CancellationToken cancellationToken) =>
{
    var result = await mediator.Send(new Read.Api.Application.Products.GetAll.Query(), cancellationToken);
    return Results.Ok(result);
});
app.UseSwagger();
app.UseSwaggerUI();
app.Run();
