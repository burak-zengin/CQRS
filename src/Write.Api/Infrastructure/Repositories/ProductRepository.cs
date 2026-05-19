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

        // Phase 1: persist aggregate changes. With strongly-typed Guid IDs already assigned in the
        // domain, we no longer rely on DB identity columns -- this also fixes the prior Id=0 bug
        // when raising creation events.
        await context.SaveChangesAsync(cancellationToken);

        // Phase 2: derive integration events from domain events and append outbox rows in the SAME transaction.
        // Each event must carry the aggregate version AT THE MOMENT IT WAS RAISED -- not the final
        // post-save version. Otherwise multi-event saves emit events that all share the final version,
        // and the consumer's replay guard (`event.Version <= existing.Version`) silently drops everything
        // except the first one.
        var outboxMessages = new List<OutboxMessage>();
        foreach (var aggregate in aggregates)
        {
            var preSaveVersion = aggregate.Version - aggregate.DomainEvents.Count;
            var eventIndex = 0;
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                eventIndex++;
                var perEventVersion = preSaveVersion + eventIndex;
                var data = IntegrationEventMapper.Map(aggregate, domainEvent, perEventVersion);
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

            // Debezium's logical decoding still captures the INSERTs in the WAL even though we
            // delete the rows immediately. This keeps the OutboxMessages table empty without
            // requiring a separate cleanup job.
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
