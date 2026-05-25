using Domain.Products;
using Domain.Products.Repositories;
using Domain.Products.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Write.Api.Application.Common;
using Write.Api.Infrastructure.Outbox;
using Write.Api.Infrastructure.Persistence;

namespace Write.Api.Infrastructure.Repositories;

public class ProductRepository(WriteDbContext context) : IProductWriteRepository
{
    public Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        context.Products.Add(product);
        return Task.CompletedTask;
    }

    public Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken)
    {
        return context.Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        var aggregates = context.ChangeTracker
            .Entries<Product>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Unchanged)
            .Select(e => e.Entity)
            .Where(p => p.DomainEvents.Count > 0)
            .ToList();

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        var outboxMessages = new List<OutboxMessage>();
        foreach (var aggregate in aggregates)
        {
            foreach (var raised in aggregate.DomainEvents)
            {
                var data = IntegrationEventMapper.Map(aggregate, raised);
                outboxMessages.Add(new OutboxMessage
                {
                    Id = data.EventId,
                    AggregateType = data.AggregateType,
                    AggregateId = data.AggregateId,
                    Type = data.Type,
                    Payload = data.Payload,
                    OccurredAt = data.OccurredAt
                });
            }
        }

        if (outboxMessages.Count > 0)
        {
            context.OutboxMessages.AddRange(outboxMessages);
            await context.SaveChangesAsync(cancellationToken);

            var ids = outboxMessages.Select(m => m.Id).ToArray();
            await context.OutboxMessages
                .Where(m => ids.Contains(m.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }
    }
}
