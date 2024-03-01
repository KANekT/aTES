using Confluent.Kafka;
using Core.Options;

namespace Core.Kafka;

public class RequestTimeConsumer : BaseConsumer<string, long>
{
    public RequestTimeConsumer(IKafkaOptions options) : base(options, Constants.KafkaTopic.RequestTime)
    {
        
    }

    protected override async Task Consume(ConsumeResult<string, long> cr, CancellationToken cancellationToken)
    {
        await Task.Run(() => Console.WriteLine($"{cr.Message.Key}: {cr.Message.Value}ms"), cancellationToken);
    }
}