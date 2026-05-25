using Confluent.Kafka;
using Consumer.Infrastructure.Messaging;
using Consumer.Infrastructure.Persistence;
using Consumer.Infrastructure.Projections;
using Consumer.Infrastructure.Repositories;
using Domain.Products.Projections;
using Domain.Products.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;

MongoConfiguration.RegisterClassMaps();

var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
    ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    ?? "Production";

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);
services.AddSingleton<IProductProjector, ProductProjector>();
services.AddSingleton<IProductListProjector, ProductListProjector>();
services.AddScoped<IProductProjectionRepository, ProductProjectionRepository>();
services.AddScoped<IntegrationEventDispatcher>();
var serviceProvider = services.BuildServiceProvider();

var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "broker:29092";
var groupId = configuration["Kafka:GroupId"] ?? "product-projection-consumer";
var topic = configuration["Kafka:Topic"] ?? "outbox.event.Product";
var mongoConnection = configuration.GetConnectionString("MongoDb") ?? "(not set)";

Console.WriteLine($"[Consumer] Environment={environment}");
Console.WriteLine($"[Consumer] Kafka={bootstrapServers}, Topic={topic}, Group={groupId}");
Console.WriteLine($"[Consumer] MongoDB={mongoConnection}");

await LogTopicDiagnosticsAsync(bootstrapServers, topic);

var cancellationSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cancellationSource.Cancel();
};

var consumerConfig = new ConsumerConfig
{
    GroupId = groupId,
    BootstrapServers = bootstrapServers,
    AutoOffsetReset = AutoOffsetReset.Earliest,
    EnableAutoCommit = false
};

using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
consumer.Subscribe(topic);

Console.WriteLine($"[Consumer] Listening topic '{topic}' on {bootstrapServers} (group: {groupId})...");

try
{
    while (!cancellationSource.IsCancellationRequested)
    {
        ConsumeResult<string, string>? result;
        try
        {
            result = consumer.Consume(cancellationSource.Token);
        }
        catch (OperationCanceledException)
        {
            break;
        }
        catch (ConsumeException ex)
        {
            Console.Error.WriteLine($"[Consumer] Consume error: {ex.Error.Reason}");
            continue;
        }

        if (result is null)
        {
            continue;
        }

        LogIncomingMessage(result);

        if (result.Message.Value is null)
        {
            Console.WriteLine(
                $"[Consumer] Null value at {result.Topic}[{result.Partition}]@{result.Offset} -- committing offset.");
            consumer.Commit(result);
            continue;
        }

        try
        {
            var committed = await HandleAsync(result, cancellationSource.Token);
            if (committed)
            {
                consumer.Commit(result);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Consumer] Handler error (offset {result.Offset}): {ex.Message}");
        }
    }
}
finally
{
    consumer.Close();
}

static void LogIncomingMessage(ConsumeResult<string, string> result)
{
    var headers = FormatHeaders(result.Message.Headers);
    var valuePreview = result.Message.Value is { Length: > 200 } v
        ? v[..200] + "..."
        : result.Message.Value;

    Console.WriteLine(
        $"[Consumer] Received {result.Topic}[{result.Partition}]@{result.Offset} " +
        $"key='{result.Message.Key}' valueLen={result.Message.Value?.Length ?? 0} headers=[{headers}]");
    if (valuePreview is not null)
    {
        Console.WriteLine($"[Consumer]   payload preview: {valuePreview}");
    }
}

static string FormatHeaders(Headers? headers)
{
    if (headers is null || headers.Count == 0)
    {
        return "(none)";
    }

    return string.Join(", ", headers.Select(h =>
        $"{h.Key}={Encoding.UTF8.GetString(h.GetValueBytes())}"));
}

async Task<bool> HandleAsync(ConsumeResult<string, string> result, CancellationToken cancellationToken)
{
    var headers = result.Message.Headers;

    if (!TryReadHeader(headers, "eventType", out var eventType))
    {
        Console.Error.WriteLine("[Consumer] Missing 'eventType' header -- cannot dispatch.");
        return false;
    }

    if (!TryResolveEventId(headers, result.Message.Value, out var eventId))
    {
        Console.Error.WriteLine("[Consumer] Missing event id (header 'id' and payload 'Id') -- cannot dispatch.");
        return false;
    }

    using var scope = serviceProvider.CreateScope();
    var projectionRepository = scope.ServiceProvider.GetRequiredService<IProductProjectionRepository>();
    var dispatcher = scope.ServiceProvider.GetRequiredService<IntegrationEventDispatcher>();

    if (await projectionRepository.HasProcessedAsync(eventId, cancellationToken))
    {
        Console.WriteLine($"[Consumer] Already processed event {eventId} -- committing offset.");
        return true;
    }

    await dispatcher.DispatchAsync(eventType, result.Message.Value, cancellationToken);
    await projectionRepository.MarkProcessedAsync(eventId, cancellationToken);
    return true;
}

static bool TryResolveEventId(Headers? headers, string? payload, out Guid eventId)
{
    if (TryReadHeader(headers, "id", out var idRaw) && Guid.TryParse(idRaw, out eventId))
    {
        return true;
    }

    if (TryReadHeader(headers, "Id", out idRaw) && Guid.TryParse(idRaw, out eventId))
    {
        return true;
    }

    if (string.IsNullOrWhiteSpace(payload))
    {
        eventId = default;
        return false;
    }

    try
    {
        using var doc = JsonDocument.Parse(payload);
        if (doc.RootElement.TryGetProperty("Id", out var idElement)
            && idElement.ValueKind == JsonValueKind.String
            && Guid.TryParse(idElement.GetString(), out eventId))
        {
            return true;
        }
    }
    catch (JsonException)
    {
    }

    eventId = default;
    return false;
}

static bool TryReadHeader(Headers? headers, string key, out string value)
{
    value = string.Empty;
    if (headers is null)
    {
        return false;
    }

    if (!headers.TryGetLastBytes(key, out var bytes) || bytes is null)
    {
        return false;
    }

    value = Encoding.UTF8.GetString(bytes);
    return true;
}

static async Task LogTopicDiagnosticsAsync(string bootstrapServers, string topic)
{
    try
    {
        using var admin = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = bootstrapServers
        }).Build();

        var metadata = admin.GetMetadata(topic, TimeSpan.FromSeconds(10));
        var topicMeta = metadata.Topics.FirstOrDefault(t => t.Topic == topic);

        if (topicMeta is null)
        {
            Console.WriteLine(
                $"[Consumer] WARNING: Topic '{topic}' does not exist on {bootstrapServers}. " +
                "Debezium has not published yet, or connector route/topic name is wrong.");
            return;
        }

        if (topicMeta.Partitions.Count == 0)
        {
            Console.WriteLine($"[Consumer] WARNING: Topic '{topic}' has no partitions.");
            return;
        }

        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = $"diag-{Guid.NewGuid():N}"
        }).Build();

        var watermarkTasks = topicMeta.Partitions
            .Select(p => consumer.QueryWatermarkOffsets(
                new TopicPartition(topic, p.PartitionId),
                TimeSpan.FromSeconds(10)))
            .ToArray();

        var totalMessages = watermarkTasks.Sum(w => (long)(w.High.Value - w.Low.Value));

        Console.WriteLine(
            $"[Consumer] Topic '{topic}' exists with {topicMeta.Partitions.Count} partition(s), " +
            $"~{totalMessages} message(s) in log (high-low watermarks).");

        if (totalMessages == 0)
        {
            Console.WriteLine(
                "[Consumer] Topic is EMPTY. After a Write API command, check Debezium connector " +
                "and Postgres WAL/outbox inserts (table stays empty by design).");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"[Consumer] WARNING: Could not reach Kafka at '{bootstrapServers}': {ex.Message}. " +
            "If running Consumer locally, use localhost:9092 (appsettings.Development.json).");
    }
}
