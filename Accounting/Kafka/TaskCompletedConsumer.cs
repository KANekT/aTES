using Accounting.Repositories;
using Confluent.Kafka;
using Core;
using Core.Enums;
using Core.Kafka;
using Core.Options;

namespace Accounting.Kafka;

public class TaskCompletedConsumer : BaseConsumer<string, string>
{
    private readonly IUserRepository _userRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ITransactionRepository _transactionRepository;

    public TaskCompletedConsumer(IKafkaOptions options, IUserRepository userRepository,
        ITransactionRepository transactionRepository, ITaskRepository taskRepository) : base(options, Constants.KafkaTopic.TaskCompleted)
    {
        _userRepository = userRepository;
        _transactionRepository = transactionRepository;
        _taskRepository = taskRepository;
    }

    protected override async Task Consume(ConsumeResult<string, string> result, CancellationToken cancellationToken)
    {
        var poPugId = result.Message.Key;
        var publicTaskId = result.Message.Value;
        
        var taskDto = await _taskRepository.GetByPublicId(publicTaskId, cancellationToken);
        if (taskDto == null)
        {
            throw new Exception("Task not exits");
        }

        var money = taskDto.Reward;
        await _transactionRepository.Create(poPugId, TransactionTypeEnum.Withdrawal, money, cancellationToken);

        await _userRepository.UpdateBalance(poPugId, money, cancellationToken);
    }
}