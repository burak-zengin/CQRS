using Domain.Products;
using MediatR;
using System.Reflection;
using Write.Api.Application.Products.Create;
using Write.Api.Infrustructure.Repositories;
using Write.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<WriteDbContext>();
builder.Services.AddScoped<IProductWriteRepository, ProductRepository>();
builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.MapPost("/api", async (
    IMediator mediator,
    Command command,
    CancellationToken cancellationToken) =>
{
    return await mediator.Send(command, cancellationToken);
});
app.UseSwagger();
app.UseSwaggerUI();
app.Run();