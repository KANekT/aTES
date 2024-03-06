using Confluent.Kafka;
using Core.Options;

namespace Core.Kafka;

public class RequestTimeV1Consumer : BaseConsumerProtobuf<string, Proto.V1.RequestTimeProto>
{
    public RequestTimeV1Consumer(IKafkaOptions options) : base(options, Constants.KafkaTopic.RequestTime)
    {
    }

    protected override async Task Consume(ConsumeResult<string, Proto.V1.RequestTimeProto> cr,
        CancellationToken cancellationToken)
    {
        await Task.Run(() => Console.WriteLine($"{cr.Message.Key}: {cr.Message.Value.EventVersion}ms"), cancellationToken);
    }
}

public class RequestTimeV2Consumer : BaseConsumerProtobuf<string, Proto.V2.RequestTimeProto>
{
    public RequestTimeV2Consumer(IKafkaOptions options) : base(options, Constants.KafkaTopic.RequestTime)
    {
    }

    protected override async Task Consume(ConsumeResult<string, Proto.V2.RequestTimeProto> cr,
        CancellationToken cancellationToken)
    {
        await Task.Run(() => Console.WriteLine($"{cr.Message.Key}: {cr.Message.Value.EventVersion}ms"), cancellationToken);
    }
}