using Analytics.Models;
using Analytics.Repositories;
using Confluent.Kafka;
using Core;
using Core.Enums;
using Core.Kafka;
using Core.Options;
using Proto.V1;

namespace Analytics.Kafka;

public class TransactionCreatedConsumer : BaseConsumer<string, TransactionCreatedProto>
{
    private readonly ITransactionRepository _transactionRepository;
    
    public TransactionCreatedConsumer(IKafkaOptions options, ITransactionRepository transactionRepository) : base(options, Constants.KafkaTopic.BillingStreaming)
    {
        _transactionRepository = transactionRepository;
    }

    protected override async Task Consume(ConsumeResult<string, TransactionCreatedProto> result, CancellationToken cancellationToken)
    {
        await RequestToDb(result, cancellationToken);
    }

    protected override async Task ConsumeBatch(IEnumerable<ConsumeResult<string, TransactionCreatedProto>> results, CancellationToken cancellationToken)
    {
        foreach (var result in results)
        {
            await RequestToDb(result, cancellationToken);
        }
    }

    private async Task RequestToDb(ConsumeResult<string, TransactionCreatedProto> result, CancellationToken cancellationToken)
    {
        var transactionDto = await _transactionRepository.GetByPublicId(result.Message.Value.PublicId, cancellationToken);
        if (transactionDto == null)
        {
            var transaction = new TransactionDto
            {
                Ulid = result.Message.Value.PublicId,
                CreatedAt = new DateTime(result.Message.Value.Time),
                Type = (TransactionTypeEnum) result.Message.Value.Type,
                Money = decimal.Parse(result.Message.Value.Money),
                PoPugId = result.Message.Value.PoPugId
            };

            await _transactionRepository.Add(transaction, cancellationToken);
        }
        else
        {
            transactionDto.Type = (TransactionTypeEnum)result.Message.Value.Type;
            transactionDto.Money = decimal.Parse(result.Message.Value.Money);
            await _transactionRepository.Update(transactionDto, cancellationToken);
        }
    }
}