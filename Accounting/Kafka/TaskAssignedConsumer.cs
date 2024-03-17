using Accounting.Models;
using Accounting.Repositories;
using Confluent.Kafka;
using Core;
using Core.Enums;
using Core.Kafka;
using Core.Options;
using Proto.V1;

namespace Accounting.Kafka;

public class TaskAssignedConsumer : BaseConsumer<string, TaskAssignedProto>
{
    private readonly IUserRepository _userRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ITransactionRepository _transactionRepository;
    
    public TaskAssignedConsumer(IKafkaOptions options, IUserRepository userRepository,
        ITransactionRepository transactionRepository, ITaskRepository taskRepository) : base(options, Constants.KafkaTopic.TaskPropertiesMutation)
    {
        _userRepository = userRepository;
        _transactionRepository = transactionRepository;
        _taskRepository = taskRepository;
    }

    protected override async Task Consume(ConsumeResult<string, TaskAssignedProto> result, CancellationToken cancellationToken)
    {
        await RequestToDb(result, cancellationToken);
    }

    protected override async Task ConsumeBatch(IEnumerable<ConsumeResult<string, TaskAssignedProto>> results, CancellationToken cancellationToken)
    {
        foreach (var result in results)
        {
            await RequestToDb(result, cancellationToken);
        }
    }

    private async Task RequestToDb(ConsumeResult<string, TaskAssignedProto> result, CancellationToken cancellationToken)
    {
        var taskDto = await _taskRepository.GetByPublicId(result.Message.Value.PublicId, cancellationToken) ??
                      await _taskRepository.Create(result.Message.Value.Time, result.Message.Value.PublicId, cancellationToken);

        taskDto.PoPugId = result.Message.Value.PoPugId;
        await _taskRepository.Update(taskDto, cancellationToken);

        var transactionType = TransactionTypeEnum.Enrollment;
        var money = -1 * taskDto.Lose;

        await _transactionRepository.Create(result.Message.Value.PoPugId, transactionType, money,
            cancellationToken);
        
        var user = await _userRepository.GetByPublicId(result.Message.Value.PublicId, cancellationToken) ??
                   await _userRepository.Create(result.Message.Value.PublicId, cancellationToken);

        user.Balance = money;
        await _userRepository.Update(user, cancellationToken);
    }
}