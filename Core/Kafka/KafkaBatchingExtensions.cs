using System.Collections.Concurrent;
using Confluent.Kafka;
using Google.Protobuf;

namespace Core.Kafka;

public static class KafkaBatchingExtensions
{
    // It is strongly recommended to only use this with consumers configured with `enable.auto.offset.store=false`
    // since some of the consumes in the batch may succeed prior to encountering an exception, without the caller
    // ever having seen the messages.
    public static IEnumerable<ConsumeResult<TKey, TVal>> ConsumeBatch<TKey, TVal>(this IConsumer<TKey, TVal> consumer,
        TimeSpan maxWaitTime, int maxBatchSize, CancellationTokenSource cts = null) where TVal : IMessage<TVal>, new()
    {
        var waitBudgetRemaining = maxWaitTime;
        var deadline = DateTime.UtcNow + waitBudgetRemaining;
        var res = new List<ConsumeResult<TKey, TVal>>();
        var resSize = 0;

        while (waitBudgetRemaining > TimeSpan.Zero && DateTime.UtcNow < deadline && resSize < maxBatchSize)
        {
            cts?.Token.ThrowIfCancellationRequested();
            var msg = consumer.Consume(waitBudgetRemaining);

            if (msg != null && !msg.IsPartitionEOF)
            {
                res.Add(msg);
                resSize++;
            }

            waitBudgetRemaining = deadline - DateTime.UtcNow;
        }

        return res;
    }

    // This override just defaults the `flushTimeout` to 5 seconds
    public static void ProduceBatch<TKey, TVal>(this IKafkaDependentProducer<TKey, TVal> producer, string topic,
        IEnumerable<Message<TKey, TVal>> messages, CancellationTokenSource cts = null) where TVal : IMessage<TVal>, new()
    {
        producer.ProduceBatch(topic, messages, TimeSpan.FromSeconds(5), cts);
    }

    private static void ProduceBatch<TKey, TVal>(this IKafkaDependentProducer<TKey, TVal> producer, string topic,
        IEnumerable<Message<TKey, TVal>> messages, TimeSpan flushTimeout, CancellationTokenSource cts = null) where TVal : IMessage<TVal>, new()
    {
        var errorReports = new ConcurrentQueue<DeliveryReport<TKey, TVal>>();
        var reportsExpected = 0;
        var reportsReceived = 0;

        void DeliveryHandler(DeliveryReport<TKey, TVal> report)
        {
            Interlocked.Increment(ref reportsReceived);

            if (report.Error.IsError)
            {
                errorReports.Enqueue(report);
            }
        }

        foreach (var message in messages)
        {
            producer.Produce(topic, message, DeliveryHandler);
            reportsExpected++;
        }

        var deadline = DateTime.UtcNow + flushTimeout;
        const int flushWaitMs = 100;

        while (DateTime.UtcNow < deadline && reportsReceived < reportsExpected)
        {
            cts?.Token.ThrowIfCancellationRequested();
            producer.Flush(TimeSpan.FromMilliseconds(flushWaitMs));
        }

        if (!errorReports.IsEmpty)
        {
            throw new AggregateException($"{errorReports.Count} Kafka produce(s) failed. Up to 10 inner exceptions follow.",
                errorReports.Take(10).Select(i => new KafkaProduceException(
                    $"A Kafka produce error occurred. Topic: {topic}, Message key: {i.Message.Key}, Code: {i.Error.Code}, Reason: " +
                    $"{i.Error.Reason}, IsBroker: {i.Error.IsBrokerError}, IsLocal: {i.Error.IsLocalError}, IsFatal: {i.Error.IsFatal}"
                ))
            );
        }

        if (reportsReceived < reportsExpected)
        {
            var msg = $"Kafka producer flush did not complete within the timeout; only received {reportsReceived} " +
                      $"delivery reports out of {reportsExpected} expected.";
            throw new KafkaProduceException(msg);
        }
    }
}

public class KafkaProduceException : Exception
{
    public KafkaProduceException(string msg)
    {
        throw new Exception(msg);
    }
}