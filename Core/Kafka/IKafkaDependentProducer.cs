using Confluent.Kafka;
using Google.Protobuf;

namespace Core.Kafka;

public interface IKafkaDependentProducer<K,V> where V : IMessage<V>, new()
{
    /// <summary>
    ///     Asynchronously produce a message and expose delivery information
    ///     via the provided callback function. Use this method of producing
    ///     if you would like flow of execution to continue immediately, and
    ///     handle delivery information out-of-band.
    /// </summary>
    public void Produce(string topic, Message<K, V> message, Action<DeliveryReport<K, V>> deliveryHandler = null);

    public void Flush(TimeSpan timeout);
}