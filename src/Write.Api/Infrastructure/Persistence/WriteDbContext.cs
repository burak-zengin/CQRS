using Domain.Products;
using Domain.Products.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Write.Api.Infrastructure.Outbox;

namespace Write.Api.Infrastructure.Persistence;

public class WriteDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public WriteDbContext(IConfiguration configuration)
    {
        _configuration = configuration;

        Database.EnsureCreated();
    }

    public DbSet<Product> Products { get; set; } = default!;

    public DbSet<ProductVariant> ProductVariants { get; set; } = default!;

    public DbSet<OutboxMessage> OutboxMessages { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("PostgreSql"));

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Id)
                .HasConversion(
                    id => id.Value,
                    value => ProductId.From(value))
                .ValueGeneratedNever();

            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);

            entity.Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(p => p.CreatedAt).IsRequired();
            entity.Property(p => p.UpdatedAt).IsRequired();

            entity.Property(p => p.Version)
                .IsConcurrencyToken();

            entity.Ignore(p => p.DomainEvents);

            entity.HasMany(p => p.Variants)
                .WithOne()
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Metadata.FindNavigation(nameof(Product.Variants))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(v => v.Id);

            entity.Property(v => v.Id)
                .HasConversion(
                    id => id.Value,
                    value => VariantId.From(value))
                .ValueGeneratedNever();

            entity.Property(v => v.ProductId)
                .HasConversion(
                    id => id.Value,
                    value => ProductId.From(value));

            entity.Property(v => v.Color).IsRequired().HasMaxLength(50);
            entity.Property(v => v.Size).IsRequired().HasMaxLength(50);

            entity.Property(v => v.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(v => v.Sku)
                .IsRequired()
                .HasMaxLength(32)
                .HasConversion(
                    sku => sku.Value,
                    value => Sku.FromPersistence(value));

            entity.Property(v => v.Barcode)
                .IsRequired()
                .HasMaxLength(14)
                .HasConversion(
                    barcode => barcode.Value,
                    value => Barcode.FromPersistence(value));

            entity.OwnsOne(v => v.Price, price =>
            {
                price.Property(p => p.Amount)
                    .HasColumnName("PriceAmount")
                    .HasColumnType("numeric(18,2)")
                    .IsRequired();

                price.Property(p => p.Currency)
                    .HasColumnName("PriceCurrency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            entity.Navigation(v => v.Price).IsRequired();

            entity.HasIndex(v => new { v.ProductId, v.Sku }).IsUnique();
            entity.HasIndex(v => new { v.ProductId, v.Barcode }).IsUnique();
            entity.HasIndex(v => v.Barcode).IsUnique();
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("outbox_messages");
            entity.HasKey(m => m.Id);
            entity.Property(m => m.AggregateType).IsRequired().HasMaxLength(100);
            entity.Property(m => m.AggregateId).IsRequired().HasMaxLength(100);
            entity.Property(m => m.Type).IsRequired().HasMaxLength(100);
            entity.Property(m => m.Payload).IsRequired().HasColumnType("text");

            entity.Property(m => m.OccurredAt)
                .HasConversion(
                    v => DateTime.SpecifyKind(v.UtcDateTime, DateTimeKind.Unspecified),
                    v => new DateTimeOffset(DateTime.SpecifyKind(v, DateTimeKind.Utc)))
                .HasColumnType("timestamp without time zone")
                .IsRequired();

            entity.HasIndex(m => m.OccurredAt);
        });

        base.OnModelCreating(modelBuilder);
    }
}
