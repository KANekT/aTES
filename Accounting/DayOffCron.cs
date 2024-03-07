using Accounting.Repositories;
using Confluent.Kafka;
using Core;
using Core.Enums;
using Core.Kafka;
using Proto;
using Proto.V1;

namespace Accounting;

public class DayOffCron : BackgroundService
{
    private readonly ILogger<DayOffCron> _logger;
    private readonly IUserRepository _userRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IKafkaDependentProducer<Null, AccountBalanceProto> _producerAccountBalance;

    public DayOffCron(
        ILogger<DayOffCron> logger,
        IUserRepository userRepository,
        ITransactionRepository transactionRepository,
        IKafkaDependentProducer<Null, AccountBalanceProto> producerAccountBalance)
    {
        _logger = logger;
        _userRepository = userRepository;
        _transactionRepository = transactionRepository;
        _producerAccountBalance = producerAccountBalance;
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        return Task.Run(async () => await StartJobLoop(cancellationToken), cancellationToken);
    }

    private async Task StartJobLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (DateTime.UtcNow.Hour != 0 || DateTime.UtcNow.Minute != 0)
                continue;

            var topMoney = await _transactionRepository.GetTopMoney(DateTime.UtcNow.Date, cancellationToken);
            //ToDo: сделать отправку события в Аналитику
            
            var users = await _userRepository.GetAll(cancellationToken);
            foreach (var user in users)
            {
                if (user.Balance > 0)
                {
                    user.Balance = 0;
                    await _userRepository.Update(user, cancellationToken);

                    await _transactionRepository.Create(
                        user.Ulid,
                        TransactionTypeEnum.Reward,
                        user.Balance,
                        cancellationToken
                    );
                    
                    //ToDo: сделать отправку на почту
                }
                
                var value = new AccountBalanceProto
                {
                    Base = new BaseProto
                    {
                        EventId = Guid.NewGuid().ToString("N"),
                        EventName = Constants.KafkaEvent.AccountBalance,
                        EventTime = DateTime.UtcNow.ToString("u"),
                        EventVersion = "1"
                    },
                    PublicId = user.Ulid,
                    Balance = user.Balance.ToString("F")
                };
        
                _producerAccountBalance.Produce(
                    Constants.KafkaTopic.AccountBalanceChange,
                    new Message<Null, AccountBalanceProto> { Value = value },
                    _deliveryReportHandlerAccountBalance
                );
            }
        }
    }
    
    private void _deliveryReportHandlerAccountBalance(DeliveryReport<Null, AccountBalanceProto> deliveryReport)
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