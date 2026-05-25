using Domain.Products.Repositories;
using MediatR;
using Microsoft.OpenApi;
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
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Read API",
        Version = "v1",
        Description = "Product read (query) operations"
    });
});

var app = builder.Build();
app.MapGet("/api/{id:guid}", async (
    Guid id,
    IMediator mediator,
    CancellationToken cancellationToken) =>
{
    var result = await mediator.Send(new Read.Api.Application.Products.Get.Query(id), cancellationToken);
    return result is null ? Results.NotFound() : Results.Ok(result);
})
.WithTags("Products")
.WithSummary("Get product details")
.WithDescription("Returns the details of the product with the given id. Returns 404 if not found.");

app.MapGet("/api", async (
    IMediator mediator,
    CancellationToken cancellationToken) =>
{
    var result = await mediator.Send(new Read.Api.Application.Products.GetAll.Query(), cancellationToken);
    return Results.Ok(result);
})
.WithTags("Products")
.WithSummary("List products")
.WithDescription("Returns a summary list of all products.");

app.UseSwagger();
app.UseSwaggerUI();
app.Run();
