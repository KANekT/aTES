using Confluent.Kafka;
using Core;
using Core.Enums;
using Core.Extensions;
using Core.Kafka;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proto;
using Proto.V1;
using Tasks.Models;
using Tasks.Repositories;

namespace Tasks.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class TaskController : ControllerBase
{
    private readonly IKafkaDependentProducer<string, Proto.V2.TaskCreatedProto> _producerTaskCreated;
    private readonly IKafkaDependentProducer<string, TaskAssignProto> _producerTaskAssign;
    private readonly IKafkaDependentProducer<string, TaskCompletedProto> _producerTaskCompleted;
    private readonly ILogger<TaskController> _logger;
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;

    public TaskController(
        IKafkaDependentProducer<string, Proto.V2.TaskCreatedProto> producerTaskCreated,
        IKafkaDependentProducer<string, TaskAssignProto> producerTaskAssign,
        IKafkaDependentProducer<string, TaskCompletedProto> producerTaskCompleted,
        ILogger<TaskController> logger,
        ITaskRepository userRepository,
        IUserRepository userRepository1)
    {
        _producerTaskCreated = producerTaskCreated;
        _producerTaskAssign = producerTaskAssign;
        _producerTaskCompleted = producerTaskCompleted;
        _logger = logger;
        _taskRepository = userRepository;
        _userRepository = userRepository1;
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> My(
        CancellationToken cancellationToken
    )
    {
        var userName = CheckUser();
        if (string.IsNullOrEmpty(userName))
        {
            return Unauthorized();
        }

        var tasks = await _taskRepository.My(userName, cancellationToken);
        return Ok(tasks);
    }
    
    [HttpPost("[action]")]
    public async Task<IActionResult> Create(
        [FromBody] TaskCreateFormModel model,
        CancellationToken cancellationToken
    )
    {
        var userName = CheckUser();
        if (string.IsNullOrEmpty(userName))
        {
            return Unauthorized();
        }
        
        if (model.Title.Contains('[') || model.Title.Contains(']'))
        {
            return BadRequest("Task contains jira id");
        }
        
        var poPugId = await _userRepository.GetRandomPoPugId(cancellationToken);

        var task = await _taskRepository.Create(model, poPugId, cancellationToken);
        if (task == null)
            return BadRequest("task not created");
        
        var value = new Proto.V2.TaskCreatedProto
        {
            Base = new BaseProto
            {
                EventId = Guid.NewGuid().ToString("N"),
                EventName = Constants.KafkaEvent.TaskCreated,
                EventTime = DateTime.UtcNow.ToString("u"),
                EventVersion = "2"
            },
            PublicId = task.Ulid,
            Title = model.Title,
            JiraId = model.JiraId,
            PoPugId = poPugId
        };
            
        _producerTaskCreated.Produce(
            Constants.KafkaTopic.TaskStreaming,
            new Message<string, Proto.V2.TaskCreatedProto> { Key = userName, Value = value },
            _deliveryReportHandlerTaskCreated
        );
        
        await AssignTask(userName, task, cancellationToken);

        return Ok();
    }

    [AuthorizeRoles(RoleEnum.Admin, RoleEnum.Manager)]
    [HttpPost("[action]")]
    public async Task<IActionResult> Shuffled(CancellationToken cancellationToken)
    {
        var userName = CheckUser();
        if (string.IsNullOrEmpty(userName))
        {
            return Unauthorized();
        }
        
        var tasks = await _taskRepository.GetAllOpen(cancellationToken);
        foreach (var task in tasks)
        {
            var poPugId = await _userRepository.GetRandomPoPugId(cancellationToken);
            task.PoPugId = poPugId;
            
            await _taskRepository.Update(task, cancellationToken);
        }

        await AssignTasks(userName, tasks, cancellationToken);
        
        return Ok();
    }

    [AuthorizeRoles(RoleEnum.PoPug, RoleEnum.Developer)]
    [HttpPut("[action]/{id}")]
    public async Task<IActionResult> Completed(long id, CancellationToken cancellationToken)
    {
        var userName = CheckUser();
        if (string.IsNullOrEmpty(userName))
        {
            return Unauthorized();
        }

        var taskPublicId = await _taskRepository.Completed(id, userName, cancellationToken);

        var value = new TaskCompletedProto
        {
            Base = new BaseProto
            {
                EventId = Guid.NewGuid().ToString("N"),
                EventName = Constants.KafkaEvent.TaskCompleted,
                EventTime = DateTime.UtcNow.ToString("u"),
                EventVersion = "1"
            },
            PublicId = taskPublicId,
            PoPugId = userName,
            Status = (int)TaskMutationEnum.Completed
        };
        
        _producerTaskCompleted.Produce(
            Constants.KafkaTopic.TaskPropertiesMutation,
            new Message<string, TaskCompletedProto> { Key = userName, Value = value },
            _deliveryReportHandlerTaskCompleted
        );

        return Ok();
    }
    
    private async Task AssignTask(string identityName, TaskDto task, CancellationToken cancellationToken)
    {
        var value = new TaskAssignProto
        {
            Base = new BaseProto
            {
                EventId = Guid.NewGuid().ToString("N"),
                EventName = Constants.KafkaEvent.TaskAssigned,
                EventTime = DateTime.UtcNow.ToString("u"),
                EventVersion = "1"
            },
            PublicId = task.Ulid,
            PoPugId = task.PoPugId,
            Status = (int)TaskMutationEnum.Assign
        };
        
        _producerTaskAssign.Produce(
            Constants.KafkaTopic.TaskPropertiesMutation,
            new Message<string, TaskAssignProto> { Key = identityName, Value = value },
            _deliveryReportHandlerTaskAssign
        );
    }
    
    private async Task AssignTasks(string identityName, TaskDto[] tasks, CancellationToken cancellationToken)
    {
        var messages = tasks.Select(task => new Message<string, TaskAssignProto>
        {
            Key = identityName, Value =
                new TaskAssignProto
                {
                    Base = new BaseProto
                    {
                        EventId = Guid.NewGuid().ToString("N"),
                        EventName = Constants.KafkaEvent.TaskAssigned,
                        EventTime = DateTime.UtcNow.ToString("u"),
                        EventVersion = "1"
                    },
                    PublicId = task.Ulid,
                    PoPugId = task.PoPugId,
                    Status = (int)TaskMutationEnum.Assign
                }
        }).ToList();
        
        // copy from https://github.com/confluentinc/confluent-kafka-dotnet/issues/890
        _producerTaskAssign.ProduceBatch(Constants.KafkaTopic.TaskPropertiesMutation, messages);

        /*
        var semaphore = new SemaphoreSlim(0, messages.Count);

        void DeliveryHandler(DeliveryReport<string, string> deliveryReport)
        {
            semaphore.Release();
        }

        messages.ForEach(message => _producer.Produce(Constants.KafkaTopic.TaskPropertyMutation, message, DeliveryHandler));
        
        await semaphore.WaitAsync(cancellationToken);
        */
    }
    
    private string? CheckUser()
    {
        var identity = HttpContext.User.Identity;
        return identity is not { IsAuthenticated: true } ? null : identity.Name;
    }
    
    private void _deliveryReportHandlerTaskCreated(DeliveryReport<string, Proto.V2.TaskCreatedProto> deliveryReport)
    {
        if (deliveryReport.Status == PersistenceStatus.NotPersisted)
        {
            // It is common to write application logs to Kafka (note: this project does not provide
            // an example logger implementation that does this). Such an implementation should
            // ideally fall back to logging messages locally in the case of delivery problems.
            _logger.Log(
                LogLevel.Warning,
                $"Message delivery failed: {deliveryReport.Message.Value}"
            );
        }
    }
    private void _deliveryReportHandlerTaskAssign(DeliveryReport<string, TaskAssignProto> deliveryReport)
    {
        if (deliveryReport.Status == PersistenceStatus.NotPersisted)
        {
            // It is common to write application logs to Kafka (note: this project does not provide
            // an example logger implementation that does this). Such an implementation should
            // ideally fall back to logging messages locally in the case of delivery problems.
            _logger.Log(
                LogLevel.Warning,
                $"Message delivery failed: {deliveryReport.Message.Value}"
            );
        }
    }
    private void _deliveryReportHandlerTaskCompleted(DeliveryReport<string, TaskCompletedProto> deliveryReport)
    {
        if (deliveryReport.Status == PersistenceStatus.NotPersisted)
        {
            // It is common to write application logs to Kafka (note: this project does not provide
            // an example logger implementation that does this). Such an implementation should
            // ideally fall back to logging messages locally in the case of delivery problems.
            _logger.Log(
                LogLevel.Warning,
                $"Message delivery failed: {deliveryReport.Message.Value}"
            );
        }
    }
}
