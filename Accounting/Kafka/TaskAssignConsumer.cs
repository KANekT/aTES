using Accounting.Models;
using Accounting.Repositories;
using Confluent.Kafka;
using Core;
using Core.Enums;
using Core.Kafka;
using Core.Options;
using Proto.V1;

namespace Accounting.Kafka;

public class TaskAssignConsumer : BaseConsumer<string, TaskAssignProto>
{
    private readonly IUserRepository _userRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ITransactionRepository _transactionRepository;
    
    public TaskAssignConsumer(IKafkaOptions options, IUserRepository userRepository,
        ITransactionRepository transactionRepository, ITaskRepository taskRepository) : base(options, Constants.KafkaTopic.TaskPropertiesMutation)
    {
        _userRepository = userRepository;
        _transactionRepository = transactionRepository;
        _taskRepository = taskRepository;
    }

    protected override async Task Consume(ConsumeResult<string, TaskAssignProto> result, CancellationToken cancellationToken)
    {
        await RequestToDb(result, cancellationToken);
    }

    protected override async Task ConsumeBatch(IEnumerable<ConsumeResult<string, TaskAssignProto>> results, CancellationToken cancellationToken)
    {
        foreach (var result in results)
        {
            await RequestToDb(result, cancellationToken);
        }
    }

    private async Task RequestToDb(ConsumeResult<string, TaskAssignProto> result, CancellationToken cancellationToken)
    {
        var taskDto = await _taskRepository.GetByPublicId(result.Message.Value.PublicId, cancellationToken) ??
                      await _taskRepository.Create(new TaskDto
                      {
                          CreatedAt = DateTime.UtcNow,
                          EditedAt = DateTime.UtcNow,
                          Ulid = result.Message.Value.PublicId,
                          PoPugId = result.Message.Value.PoPugId,
                          Title = string.Empty
                      }, cancellationToken);

        var transactionType = TransactionTypeEnum.Enrollment;
        decimal money = -1 * taskDto.Lose;
        
        await _transactionRepository.Create(result.Message.Value.PoPugId, transactionType, money, cancellationToken);
        
        await _userRepository.UpdateBalance(result.Message.Value.PoPugId, money, cancellationToken);
    }
}