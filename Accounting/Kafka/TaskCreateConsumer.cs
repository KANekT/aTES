using Accounting.Models;
using Accounting.Repositories;
using Confluent.Kafka;
using Core;
using Core.Enums;
using Core.Kafka;
using Core.Options;
using Proto.V1;

namespace Accounting.Kafka;

public class TaskCreateConsumer : BaseConsumer<string, TaskCreatedProto>
{
    private readonly IUserRepository _userRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ITransactionRepository _transactionRepository;

    public TaskCreateConsumer(IKafkaOptions options, IUserRepository userRepository,
        ITransactionRepository transactionRepository, ITaskRepository taskRepository) : base(options, Constants.KafkaTopic.TaskStreaming)
    {
        _userRepository = userRepository;
        _transactionRepository = transactionRepository;
        _taskRepository = taskRepository;
    }

    protected override async Task Consume(ConsumeResult<string, TaskCreatedProto> result, CancellationToken cancellationToken)
    {
        await RequestToDb(result, cancellationToken);
    }

    protected override async Task ConsumeBatch(IEnumerable<ConsumeResult<string, TaskCreatedProto>> results, CancellationToken cancellationToken)
    {
        foreach (var result in results)
        {
            await RequestToDb(result, cancellationToken);
        }
    }
    
    private async Task RequestToDb(ConsumeResult<string, TaskCreatedProto> result, CancellationToken cancellationToken)
    {
        var taskDto = await _taskRepository.GetByPublicId(result.Message.Value.PublicId, cancellationToken);
        if (taskDto == null)
        {
            var task = new TaskDto
            {
                Ulid = result.Message.Value.PublicId,
                CreatedAt = DateTime.UtcNow,
                EditedAt = DateTime.UtcNow,
                Title = result.Message.Value.Title,
                PoPugId = result.Message.Value.PoPugId
            };

            taskDto = await _taskRepository.Create(task, cancellationToken);
        }
        else
        {
            taskDto.Title = result.Message.Value.Title;
            await _taskRepository.Update(taskDto, cancellationToken);
        }
        
        var money = -1 * taskDto.Lose;
        await _transactionRepository.Create(result.Message.Value.PoPugId, TransactionTypeEnum.Init, money, cancellationToken);

        await _userRepository.UpdateBalance(result.Message.Value.PoPugId, money, cancellationToken);
    }
}