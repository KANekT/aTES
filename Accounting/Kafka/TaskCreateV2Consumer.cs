using Accounting.Models;
using Accounting.Repositories;
using Confluent.Kafka;
using Core;
using Core.Enums;
using Core.Kafka;
using Core.Options;
using Proto;
using Proto.V2;

namespace Accounting.Kafka;

public class TaskCreateV2Consumer : BaseConsumer<string, TaskCreatedProto>
{
    private readonly IUserRepository _userRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ITransactionRepository _transactionRepository;
    
    private readonly IKafkaDependentProducer<string, Proto.V1.TaskPriceSetProto> _producerTaskPriceSet;
    
    public TaskCreateV2Consumer(IKafkaOptions options, IUserRepository userRepository,
        ITransactionRepository transactionRepository, ITaskRepository taskRepository, IKafkaDependentProducer<string, Proto.V1.TaskPriceSetProto> producerTaskPriceSet) : base(options, Constants.KafkaTopic.TaskStreaming)
    {
        _userRepository = userRepository;
        _transactionRepository = transactionRepository;
        _taskRepository = taskRepository;
        
        _producerTaskPriceSet = producerTaskPriceSet;
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
                CreatedAt = new DateTime(result.Message.Value.Time),
                EditedAt = DateTime.UtcNow,
                Title = result.Message.Value.Title,
                JiraId = result.Message.Value.JiraId,
                PoPugId = result.Message.Value.PoPugId
            };

            taskDto = await _taskRepository.Create(task, cancellationToken);
            
            var value = new Proto.V1.TaskPriceSetProto
            {
                Base = new BaseProto
                {
                    EventId = Guid.NewGuid().ToString("N"),
                    EventName = Constants.KafkaEvent.TaskPriceSet,
                    EventTime = DateTime.UtcNow.ToString("u"),
                    EventVersion = "1"
                },
                PublicId = task.Ulid,
                PoPugId = result.Message.Value.PoPugId,
                Lose = task.Lose.ToString("F"),
                Reward = task.Reward.ToString("F"),
                Time = task.CreatedAt.Ticks
            };
        
            try {
                await _producerTaskPriceSet.ProduceAsync(
                    Constants.KafkaTopic.TaskPropertiesMutation,
                    new Message<string, Proto.V1.TaskPriceSetProto> { Key = task.Ulid, Value = value }
                );
            }
            catch (Exception ex)
            {
                // Add Error to DB
            }
        }
        else
        {
            taskDto.EditedAt = DateTime.UtcNow;
            taskDto.Title = result.Message.Value.Title;
            await _taskRepository.Update(taskDto, cancellationToken);
        }

        var money = -1 * taskDto.Lose;
        await _transactionRepository.Create(result.Message.Value.PoPugId, TransactionTypeEnum.Init, money,
            cancellationToken);

        await _userRepository.UpdateBalance(result.Message.Value.PoPugId, money, cancellationToken);
    }
}