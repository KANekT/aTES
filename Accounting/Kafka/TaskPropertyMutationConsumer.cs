using Accounting.Repositories;
using Confluent.Kafka;
using Core;
using Core.Enums;
using Core.EventModels;
using Core.Extensions;
using Core.Kafka;
using Core.Options;

namespace Accounting.Kafka;

public class TaskPropertyMutationConsumer : BaseConsumer<string, string>
{
    private readonly IUserRepository _userRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ITransactionRepository _transactionRepository;

    public TaskPropertyMutationConsumer(IKafkaOptions options, IUserRepository userRepository,
        ITransactionRepository transactionRepository, ITaskRepository taskRepository) : base(options, Constants.KafkaTopic.TaskPropertiesMutation)
    {
        _userRepository = userRepository;
        _transactionRepository = transactionRepository;
        _taskRepository = taskRepository;
    }

    protected override async Task Consume(ConsumeResult<string, string> result, CancellationToken cancellationToken)
    {
        await RequestToDb(result, cancellationToken);
    }

    protected override async Task ConsumeBatch(IEnumerable<ConsumeResult<string, string>> results, CancellationToken cancellationToken)
    {
        foreach (var result in results)
        {
            await RequestToDb(result, cancellationToken);
        }
    }
    
    private async Task RequestToDb(ConsumeResult<string, string> result, CancellationToken cancellationToken)
    {
        var task = result.Message.Value.Encode<TaskMutationEventModel>();

        var taskDto = await _taskRepository.GetByPublicId(task.PublicTaskId, cancellationToken) ??
                      await _taskRepository.Create(new TaskCreatedEventModel
                      {
                          PublicId = task.PublicTaskId,
                          PoPugId = task.PublicPoPugId,
                          Description = string.Empty
                      }, cancellationToken);

        decimal money;
        TransactionTypeEnum transactionType;
        switch (task.Status)
        {
            case TaskMutationEnum.Assign:
                transactionType = TransactionTypeEnum.Enrollment;
                money = -1 * taskDto.Lose;
                break;
            case TaskMutationEnum.Completed:
                transactionType = TransactionTypeEnum.Withdrawal;
                money = taskDto.Reward;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        await _transactionRepository.Create(task.PublicPoPugId, transactionType, money, cancellationToken);

        await _userRepository.UpdateBalance(task.PublicPoPugId, money, cancellationToken);
    }
}