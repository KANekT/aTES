using Confluent.Kafka;
using Core.Options;
using Proto.V1;

namespace Core.Kafka;

public class RequestTimeV1Consumer : BaseConsumer<string, Proto.V1.RequestTimeProto>
{
    public RequestTimeV1Consumer(IKafkaOptions options) : base(options, Constants.KafkaTopic.RequestTime)
    {
    }

    protected override async Task Consume(ConsumeResult<string, Proto.V1.RequestTimeProto> cr,
        CancellationToken cancellationToken)
    {
        await Task.Run(() => Console.WriteLine($"{cr.Message.Key}: {cr.Message.Value.EventVersion}ms"), cancellationToken);
    }

    protected override Task ConsumeBatch(IEnumerable<ConsumeResult<string, RequestTimeProto>> results, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

public class RequestTimeV2Consumer : BaseConsumer<string, Proto.V2.RequestTimeProto>
{
    public RequestTimeV2Consumer(IKafkaOptions options) : base(options, Constants.KafkaTopic.RequestTime)
    {
    }

    protected override async Task Consume(ConsumeResult<string, Proto.V2.RequestTimeProto> cr,
        CancellationToken cancellationToken)
    {
        await Task.Run(() => Console.WriteLine($"{cr.Message.Key}: {cr.Message.Value.EventVersion}ms"), cancellationToken);
    }

    protected override Task ConsumeBatch(IEnumerable<ConsumeResult<string, Proto.V2.RequestTimeProto>> results, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}