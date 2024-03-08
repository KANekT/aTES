using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Google.Protobuf;

namespace Core.Kafka;

public class KafkaDependentProducer<K, V> : IKafkaDependentProducer<K, V> where V : IMessage<V>, new()
{
    IProducer<K, V>? kafkaHandle;
    private KafkaClientHandle _handle;

    public KafkaDependentProducer(KafkaClientHandle handle)
    {
        _handle = handle;
    }

    /// <summary>
    ///     Asychronously produce a message and expose delivery information
    ///     via the returned Task. Use this method of producing if you would
    ///     like to await the result before flow of execution continues.
    /// <summary>
    public Task? ProduceAsync(string topic, Message<K, V> message)
        => kafkaHandle?.ProduceAsync(topic, message);

    /// <summary>
    ///     Asynchronously produce a message and expose delivery information
    ///     via the provided callback function. Use this method of producing
    ///     if you would like flow of execution to continue immediately, and
    ///     handle delivery information out-of-band.
    /// </summary>
    public void Produce(string topic, Message<K, V> message, Action<DeliveryReport<K, V>> deliveryHandler = null)
    {
        var schemaRegistryConfig = new SchemaRegistryConfig
        {
            // Note: you can specify more than one schema registry url using the
            // schema.registry.url property for redundancy (comma separated list). 
            // The property name is not plural to follow the convention set by
            // the Java implementation.
            Url = "http://localhost:8081",
        };

        using (var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig))
        {
            kafkaHandle = new DependentProducerBuilder<K, V>(_handle.Handle)
                .SetValueSerializer(new ProtobufSerializer<V>(schemaRegistry).AsSyncOverAsync())
                .Build();
            kafkaHandle.Produce(topic, message, deliveryHandler);
        }
    }

    public void Flush(TimeSpan timeout)
        => kafkaHandle?.Flush(timeout);
}