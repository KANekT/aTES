using Analytics.Models;
using Analytics.Repositories;
using Confluent.Kafka;
using Core;
using Core.Kafka;
using Core.Options;
using Proto.V1;

namespace Analytics.Kafka;

public class TaskCreatedConsumer : BaseConsumer<string, TaskCreatedProto>
{
    private readonly ITaskRepository _taskRepository;
    
    public TaskCreatedConsumer(IKafkaOptions options, ITaskRepository taskRepository) : base(options, Constants.KafkaTopic.TaskStreaming)
    {
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
                CreatedAt = new DateTime(result.Message.Value.Time),
                EditedAt = DateTime.UtcNow,
                Title = result.Message.Value.Title,
                PoPugId = result.Message.Value.PoPugId
            };

            await _taskRepository.Create(task, cancellationToken);
        }
        else
        {
            taskDto.EditedAt = DateTime.UtcNow;
            taskDto.Title = result.Message.Value.Title;
            await _taskRepository.Update(taskDto, cancellationToken);
        }
    }
}