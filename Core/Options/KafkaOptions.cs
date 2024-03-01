using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace Core.Options;

public class KafkaOptions : OptionsProviderBase, IKafkaOptions
{
    protected override string MainSectionName => "Kafka";

    public KafkaOptions(IConfiguration configuration) : base(configuration)
    {
        GetSection("ConsumerSettings").Bind(ConsumerConfig);
    }

    public ConsumerConfig ConsumerConfig { get; } = new();
}
