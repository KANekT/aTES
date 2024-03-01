using Confluent.Kafka;

namespace Core.Options;

public interface IKafkaOptions
{
    ConsumerConfig ConsumerConfig { get; }
}