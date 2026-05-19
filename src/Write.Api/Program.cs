using Domain.Products.Repositories;
using FluentValidation;
using MediatR;
using System.Reflection;
using Write.Api.Application.Behaviors;
using Write.Api.Infrastructure.ExceptionHandling;
using Write.Api.Infrastructure.Persistence;
using Write.Api.Infrastructure.Repositories;
using CreateCommand = Write.Api.Application.Products.Create.Command;
using RenameCommand = Write.Api.Application.Products.Rename.Command;
using ActivateCommand = Write.Api.Application.Products.Activate.Command;
using ArchiveCommand = Write.Api.Application.Products.Archive.Command;
using AddVariantCommand = Write.Api.Application.Products.AddVariant.Command;
using RemoveVariantCommand = Write.Api.Application.Products.RemoveVariant.Command;
using ChangePriceCommand = Write.Api.Application.Products.ChangePrice.Command;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WriteDbContext>();
builder.Services.AddScoped<IProductWriteRepository, ProductRepository>();
builder.Services.AddScoped<IBarcodeUniquenessChecker, BarcodeUniquenessChecker>();

builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();

app.MapPost("/api", async (
    IMediator mediator,
    CreateCommand command,
    CancellationToken cancellationToken) =>
{
    var id = await mediator.Send(command, cancellationToken);
    return Results.Created($"/api/{id}", new { id });
});

app.MapPut("/api/{id:guid}/name", async (
    IMediator mediator,
    Guid id,
    RenameBody body,
    CancellationToken cancellationToken) =>
{
    await mediator.Send(new RenameCommand(id, body.Name), cancellationToken);
    return Results.NoContent();
});

app.MapPost("/api/{id:guid}/activate", async (
    IMediator mediator,
    Guid id,
    CancellationToken cancellationToken) =>
{
    await mediator.Send(new ActivateCommand(id), cancellationToken);
    return Results.NoContent();
});

app.MapPost("/api/{id:guid}/archive", async (
    IMediator mediator,
    Guid id,
    CancellationToken cancellationToken) =>
{
    await mediator.Send(new ArchiveCommand(id), cancellationToken);
    return Results.NoContent();
});

app.MapPost("/api/{id:guid}/variants", async (
    IMediator mediator,
    Guid id,
    AddVariantBody body,
    CancellationToken cancellationToken) =>
{
    var variantId = await mediator.Send(
        new AddVariantCommand(
            id,
            body.Sku,
            body.Barcode,
            body.Color,
            body.Size,
            body.PriceAmount,
            body.Currency),
        cancellationToken);
    return Results.Created($"/api/{id}/variants/{variantId}", new { id = variantId });
});

app.MapDelete("/api/{id:guid}/variants/{variantId:guid}", async (
    IMediator mediator,
    Guid id,
    Guid variantId,
    CancellationToken cancellationToken) =>
{
    await mediator.Send(new RemoveVariantCommand(id, variantId), cancellationToken);
    return Results.NoContent();
});

app.MapPut("/api/{id:guid}/variants/{variantId:guid}/price", async (
    IMediator mediator,
    Guid id,
    Guid variantId,
    ChangePriceBody body,
    CancellationToken cancellationToken) =>
{
    await mediator.Send(
        new ChangePriceCommand(id, variantId, body.Amount, body.Currency),
        cancellationToken);
    return Results.NoContent();
});

app.UseSwagger();
app.UseSwaggerUI();
app.Run();

internal record RenameBody(string Name);
internal record AddVariantBody(
    string Sku,
    string Barcode,
    string Color,
    string Size,
    decimal PriceAmount,
    string Currency);
internal record ChangePriceBody(decimal Amount, string Currency);
