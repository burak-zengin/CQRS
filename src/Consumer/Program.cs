using Confluent.Kafka;
using Consumer.Infrustructure.Repositories;
using Consumer.Models;
using Domain.Products;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Write.Api.Domain.Products;

static IConfigurationRoot GetConfiguration()
{
    var builder = new ConfigurationBuilder();
    return builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true).Build();
}

static IServiceProvider ConfigureServiceCollection(IConfigurationRoot configuration)
{
    ServiceCollection serviceCollection = new();
    serviceCollection.AddSingleton<IConfiguration>(c => configuration);
    serviceCollection.AddScoped<IProductWriteRepository, ProductRepository>();
    return serviceCollection.BuildServiceProvider();
}

var serviceProvider = ConfigureServiceCollection(GetConfiguration());

CancellationTokenSource source = new();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    source.Cancel();
};

ConsumerConfig config = new()
{
    GroupId = "Consumer",
    BootstrapServers = "broker:29092",
    AutoOffsetReset = AutoOffsetReset.Earliest
};

using (IConsumer<Ignore, string> consumer = new ConsumerBuilder<Ignore, string>(config).Build())
{
    consumer.Subscribe("datatransfer.public.Products");

    while (true)
    {
        var consumeResult = consumer.Consume(source.Token);
        var message = JsonSerializer.Deserialize<Message<Product>>(consumeResult.Value);

        using (var scope = serviceProvider.CreateScope())
        {
            var productRepository = scope.ServiceProvider.GetRequiredService<IProductWriteRepository>();
            await productRepository.CreateAsync(message.payload.after, source.Token);
        }
    }
}
