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
            await _taskRepository.Create(result.Message.Value.Time, result.Message.Value.PublicId, result.Message.Value.Title, cancellationToken);
        }
        else
        {
            taskDto.EditedAt = DateTime.UtcNow;
            taskDto.Title = result.Message.Value.Title;
            await _taskRepository.Update(taskDto, cancellationToken);
        }
    }
}