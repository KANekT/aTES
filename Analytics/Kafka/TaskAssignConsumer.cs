using Analytics.Models;
using Analytics.Repositories;
using Confluent.Kafka;
using Core;
using Core.Kafka;
using Core.Options;
using Proto.V1;

namespace Analytics.Kafka;

public class TaskAssignConsumer : BaseConsumer<string, TaskAssignProto>
{
    private readonly ITaskRepository _taskRepository;
    
    public TaskAssignConsumer(IKafkaOptions options, ITaskRepository taskRepository) : base(options, Constants.KafkaTopic.TaskPropertiesMutation)
    {
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

        taskDto.PoPugId = result.Message.Value.PoPugId;
        await _taskRepository.Update(taskDto, cancellationToken);
    }
}