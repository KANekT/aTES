using Analytics.Models;
using Analytics.Repositories;
using Confluent.Kafka;
using Core;
using Core.Enums;
using Core.Kafka;
using Core.Options;
using Proto.V1;

namespace Analytics.Kafka;

public class TaskCompletedConsumer : BaseConsumer<string, TaskCompletedProto>
{
    private readonly ITaskRepository _taskRepository;
    
    public TaskCompletedConsumer(IKafkaOptions options, ITaskRepository taskRepository) : base(options, Constants.KafkaTopic.TaskPropertiesMutation)
    {
        _taskRepository = taskRepository;
    }

    protected override async Task Consume(ConsumeResult<string, TaskCompletedProto> result, CancellationToken cancellationToken)
    {
        await RequestToDb(result, cancellationToken);
    }

    protected override async Task ConsumeBatch(IEnumerable<ConsumeResult<string, TaskCompletedProto>> results, CancellationToken cancellationToken)
    {
        foreach (var result in results)
        {
            await RequestToDb(result, cancellationToken);
        }
    }

    private async Task RequestToDb(ConsumeResult<string, TaskCompletedProto> result, CancellationToken cancellationToken)
    {
        var taskDto = await _taskRepository.GetByPublicId(result.Message.Value.PublicId, cancellationToken) ??
                      await _taskRepository.Create(new TaskDto
                      {
                          Ulid = result.Message.Value.PublicId,
                          PoPugId = result.Message.Value.PoPugId,
                          Title = string.Empty
                      }, cancellationToken);

        taskDto.Status = TaskStatusEnum.Completed;
        await _taskRepository.Update(taskDto, cancellationToken);
    }
}