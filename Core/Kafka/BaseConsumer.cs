using Confluent.Kafka;
using Core.Options;
using Microsoft.Extensions.Hosting;

namespace Core.Kafka;

public abstract class BaseConsumer<TKey, TValue> : BackgroundService
{
    private readonly string _topic;
    private readonly IConsumer<TKey, TValue> _kafkaConsumer;
    
    protected BaseConsumer(IKafkaOptions options, string kafkaTopic)
    {
        _topic = kafkaTopic;
        _kafkaConsumer = new ConsumerBuilder<TKey, TValue>(options.ConsumerConfig).Build();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(async () => await StartConsumerLoop(stoppingToken), stoppingToken);
    }
        
    protected abstract Task Consume(ConsumeResult<TKey, TValue> result, CancellationToken cancellationToken);
    
    private async Task StartConsumerLoop(CancellationToken cancellationToken)
    {
        _kafkaConsumer.Subscribe(_topic);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Consume(_kafkaConsumer.Consume(cancellationToken), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException e)
            {
                // Consumer errors should generally be ignored (or logged) unless fatal.
                Console.WriteLine($"Consume error: {e.Error.Reason}");

                if (e.Error.IsFatal)
                {
                    // https://github.com/edenhill/librdkafka/blob/master/INTRODUCTION.md#fatal-consumer-errors
                    break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected error: {e}");
                break;
            }
        }
    }

    public override void Dispose()
    {
        _kafkaConsumer.Close(); // Commit offsets and leave the group cleanly.
        _kafkaConsumer.Dispose();

        base.Dispose();
    }
}