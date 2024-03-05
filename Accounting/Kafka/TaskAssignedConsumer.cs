using Accounting.Repositories;
using Confluent.Kafka;
using Core;
using Core.Enums;
using Core.EventModels;
using Core.Extensions;
using Core.Kafka;
using Core.Options;

namespace Accounting.Kafka;

public class TaskAssignedConsumer : BaseConsumer<string, string>
{
    private readonly IUserRepository _userRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ITransactionRepository _transactionRepository;

    public TaskAssignedConsumer(IKafkaOptions options, IUserRepository userRepository,
        ITransactionRepository transactionRepository, ITaskRepository taskRepository) : base(options, Constants.KafkaTopic.TaskAssigned)
    {
        _userRepository = userRepository;
        _transactionRepository = transactionRepository;
        _taskRepository = taskRepository;
    }

    protected override async Task Consume(ConsumeResult<string, string> result, CancellationToken cancellationToken)
    {
        var task = result.Message.Value.Encode<TaskAssignedEventModel>();
        
        var taskDto = await _taskRepository.GetByPublicId(task.PublicTaskId, cancellationToken);
        if (taskDto == null)
        {
            throw new Exception("Task not exits");
        }
        
        var money = -1 * taskDto.Lose;
        await _transactionRepository.Create(task.PublicPoPugId, TransactionTypeEnum.Enrollment, money, cancellationToken);

        await _userRepository.UpdateBalance(task.PublicPoPugId, money, cancellationToken);
    }
}