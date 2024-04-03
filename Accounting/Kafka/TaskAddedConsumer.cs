using Accounting.Models;
using Accounting.Repositories;
using Confluent.Kafka;
using Core;
using Core.Enums;
using Core.Kafka;
using Core.Options;
using Proto;
using Proto.V1;

namespace Accounting.Kafka;

public class TaskAddedConsumer : BaseConsumer<string, TaskAddedProto>
{
    private readonly IUserRepository _userRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ITransactionRepository _transactionRepository;
    
    private readonly IKafkaDependentProducer<string, TaskPriceSetProto> _producerTaskPriceSet;

    public TaskAddedConsumer(IKafkaOptions options, IUserRepository userRepository,
        ITransactionRepository transactionRepository, ITaskRepository taskRepository, IKafkaDependentProducer<string, TaskPriceSetProto> producerTaskPriceSet) : base(options, Constants.KafkaTopic.TaskPropertiesMutation)
    {
        _taskRepository = taskRepository;
        _producerTaskPriceSet = producerTaskPriceSet;
        _userRepository = userRepository;
        _transactionRepository = transactionRepository;
    }

    protected override async Task Consume(ConsumeResult<string, TaskAddedProto> result, CancellationToken cancellationToken)
    {
        await RequestToDb(result, cancellationToken);
    }

    protected override async Task ConsumeBatch(IEnumerable<ConsumeResult<string, TaskAddedProto>> results, CancellationToken cancellationToken)
    {
        foreach (var result in results)
        {
            await RequestToDb(result, cancellationToken);
        }
    }

    private async Task RequestToDb(ConsumeResult<string, TaskAddedProto> result, CancellationToken cancellationToken)
    {
        var taskDto = await _taskRepository.GetByPublicId(result.Message.Value.PublicId, cancellationToken) ??
                      await _taskRepository.Create(result.Message.Value.Time, result.Message.Value.PublicId, cancellationToken);

        taskDto.PoPugId = result.Message.Value.PoPugId;
        await _taskRepository.Update(taskDto, cancellationToken);
        
        var money = -1 * taskDto.Lose;
        await _transactionRepository.Create(result.Message.Value.PoPugId, TransactionTypeEnum.Init, money,
            cancellationToken);

        var user = await _userRepository.GetByPublicId(result.Message.Value.PublicId, cancellationToken) ??
                   await _userRepository.Create(result.Message.Value.PublicId, cancellationToken);

        user.Balance = money;
        await _userRepository.Update(user, cancellationToken);
        
        var value = new TaskPriceSetProto
        {
            Base = new BaseProto
            {
                EventId = Guid.NewGuid().ToString("N"),
                EventName = Constants.KafkaEvent.TaskPriceSet,
                EventTime = DateTime.UtcNow.ToString("u"),
                EventVersion = "1"
            },
            PublicId = taskDto.Ulid,
            PoPugId = result.Message.Value.PoPugId,
            Lose = taskDto.Lose.ToString("F"),
            Reward = taskDto.Reward.ToString("F"),
            Time = taskDto.CreatedAt.Ticks
        };
        
        try {
            await _producerTaskPriceSet.ProduceAsync(
                Constants.KafkaTopic.TaskPropertiesMutation,
                new Message<string, TaskPriceSetProto> { Key = taskDto.Ulid, Value = value }
            );
        }
        catch (Exception ex)
        {
            // Add Error to DB
        }
    }
}