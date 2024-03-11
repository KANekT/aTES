using Accounting.Models;
using Confluent.Kafka;
using Core;
using Core.Enums;
using Core.Kafka;
using Proto;
using Proto.V1;

namespace Accounting.Repositories;

public class TransactionRepository : GenericRepository<TransactionDto>, ITransactionRepository
{
    private readonly ILogger<TransactionRepository> _logger;
    private readonly IKafkaDependentProducer<string, TransactionCreatedProto> _producerTaskCreated;

    public TransactionRepository(DapperContext context, ILogger<TransactionRepository> logger, IKafkaDependentProducer<string, TransactionCreatedProto> producerTaskCreated) : base(context)
    {
        _logger = logger;
        _producerTaskCreated = producerTaskCreated;
    }

    public async Task<TransactionDto?> Create(string publicId, TransactionTypeEnum type, decimal money, CancellationToken cancellationToken)
    {
        var transactionDto = new TransactionDto
        {
            Ulid = Ulid.NewUlid().ToString(),
            CreatedAt = DateTime.UtcNow,
            Type = type,
            PoPugId = publicId,
            Money = money,
        };
        
        var transactionAdd = await Add(transactionDto, cancellationToken);
        if (transactionAdd)
        {
            var value = new TransactionCreatedProto
            {
                Base = new BaseProto
                {
                    EventId = Guid.NewGuid().ToString("N"),
                    EventName = Constants.KafkaEvent.TransactionCreated,
                    EventTime = DateTime.UtcNow.ToString("u"),
                    EventVersion = "1"
                },
                PublicId = transactionDto.Ulid,
                PoPugId = transactionDto.PoPugId,
                Money = transactionDto.Money.ToString("F"),
                Type = (int)transactionDto.Type
            };
            
            _producerTaskCreated.Produce(
                Constants.KafkaTopic.BillingStreaming,
                new Message<string, TransactionCreatedProto> { Key = publicId, Value = value },
                _deliveryReportHandlerTransactionCreated
            );
        }
        return transactionAdd ? transactionDto : null;
    }

    public async Task<decimal> GetTopMoney(DateTime utcNowDate, CancellationToken cancellationToken)
    {
        var all = await GetAll(cancellationToken);
        var money = all.Where(x =>
            x.CreatedAt.Date == utcNowDate &&
            x.Type is TransactionTypeEnum.Withdrawal or TransactionTypeEnum.Enrollment)
            .Sum(x => x.Money);

        return money > 0 ? money : 0;
    }
    
    private void _deliveryReportHandlerTransactionCreated(DeliveryReport<string, TransactionCreatedProto> deliveryReport)
    {
        if (deliveryReport.Status == PersistenceStatus.NotPersisted)
        {
            // It is common to write application logs to Kafka (note: this project does not provide
            // an example logger implementation that does this). Such an implementation should
            // ideally fall back to logging messages locally in the case of delivery problems.
            _logger.Log(
                LogLevel.Warning,
                $"Message delivery failed: {deliveryReport.Message.Value}"
            );
        }
    }
}