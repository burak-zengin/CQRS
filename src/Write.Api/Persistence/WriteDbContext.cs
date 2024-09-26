using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Write.Api.Domain.Products;

namespace Write.Api.Persistence;

public class WriteDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public WriteDbContext(IConfiguration configuration)
    {
        _configuration = configuration;

        Database.EnsureCreated();
    }

    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("PostgreSql"));

        base.OnConfiguring(optionsBuilder);
    }
}
