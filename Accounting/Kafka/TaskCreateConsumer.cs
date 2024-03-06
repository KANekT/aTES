using Accounting.Repositories;
using Confluent.Kafka;
using Core;
using Core.Enums;
using Core.EventModels;
using Core.Extensions;
using Core.Kafka;
using Core.Options;

namespace Accounting.Kafka;

public class TaskCreateConsumer : BaseConsumer<string, string>
{
    private readonly IUserRepository _userRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ITransactionRepository _transactionRepository;

    public TaskCreateConsumer(IKafkaOptions options, IUserRepository userRepository,
        ITransactionRepository transactionRepository, ITaskRepository taskRepository) : base(options, Constants.KafkaTopic.TaskCreatedStream)
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
        var task = result.Message.Value.Encode<TaskCreatedEventModel>();
        
        var taskDto = await _taskRepository.GetByPublicId(task.PublicId, cancellationToken);
        if (taskDto == null)
        {
            taskDto = await _taskRepository.Create(task, cancellationToken);
        }
        else
        {
            taskDto.Description = task.Description;
            await _taskRepository.Update(taskDto, cancellationToken);
        }
        
        var money = -1 * taskDto.Lose;
        await _transactionRepository.Create(task.PoPugId, TransactionTypeEnum.Init, money, cancellationToken);

        await _userRepository.UpdateBalance(task.PoPugId, money, cancellationToken);
    }
}