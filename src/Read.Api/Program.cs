using Domain.Products;
using MediatR;
using Read.Api.Infrustructure.Repositories;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});
builder.Services.AddScoped<IProductReadRepository, ProductRepository>();

var app = builder.Build();
app.MapGet("/api/{id}", async (
    int id,
    IMediator mediator,
    CancellationToken cancellationToken) =>
{
    return await mediator.Send(new Read.Api.Application.Products.Get.Query(id), cancellationToken);
});
app.MapGet("/api", async (
    IMediator mediator,
    CancellationToken cancellationToken) =>
{
    return await mediator.Send(new Read.Api.Application.Products.GetAll.Query(), cancellationToken);
});
app.Run();