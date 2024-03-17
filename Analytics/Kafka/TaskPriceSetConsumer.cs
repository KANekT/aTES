using Analytics.Models;
using Analytics.Repositories;
using Confluent.Kafka;
using Core;
using Core.Kafka;
using Core.Options;
using Proto.V1;

namespace Analytics.Kafka;

public class TaskPriceSetConsumer : BaseConsumer<string, TaskPriceSetProto>
{
    private readonly ITaskRepository _taskRepository;
    
    public TaskPriceSetConsumer(IKafkaOptions options, ITaskRepository taskRepository) : base(options, Constants.KafkaTopic.TaskStreaming)
    {
        _taskRepository = taskRepository;
    }

    protected override async Task Consume(ConsumeResult<string, TaskPriceSetProto> result, CancellationToken cancellationToken)
    {
        await RequestToDb(result, cancellationToken);
    }

    protected override async Task ConsumeBatch(IEnumerable<ConsumeResult<string, TaskPriceSetProto>> results, CancellationToken cancellationToken)
    {
        foreach (var result in results)
        {
            await RequestToDb(result, cancellationToken);
        }
    }

    private async Task RequestToDb(ConsumeResult<string, TaskPriceSetProto> result, CancellationToken cancellationToken)
    {
        var taskDto = await _taskRepository.GetByPublicId(result.Message.Value.PublicId, cancellationToken);
        if (taskDto == null)
        {
            var task = new TaskDto
            {
                Ulid = result.Message.Value.PublicId,
                CreatedAt = new DateTime(result.Message.Value.Time),
                EditedAt = DateTime.UtcNow,
                Lose = decimal.Parse(result.Message.Value.Lose),
                Reward = decimal.Parse(result.Message.Value.Reward),
                PoPugId = result.Message.Value.PoPugId
            };

            await _taskRepository.Create(task, cancellationToken);
        }
        else
        {
            taskDto.EditedAt = DateTime.UtcNow;
            taskDto.Lose = decimal.Parse(result.Message.Value.Lose);
            taskDto.Reward = decimal.Parse(result.Message.Value.Reward);

            await _taskRepository.Update(taskDto, cancellationToken);
        }
    }
}