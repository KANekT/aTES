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

    public DayOffCron(
        ILogger<DayOffCron> logger,
        IUserRepository userRepository,
        ITransactionRepository transactionRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
        _transactionRepository = transactionRepository;
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
                    
                    //ToDo: сделать отправку письма
                    //счастья о выплате зарплаты
                    
                }
            }
        }
    }
}