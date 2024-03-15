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
    private readonly IKafkaDependentProducer<string, TaskCreatedProto> _producerTaskCreatedV1;
    private readonly IKafkaDependentProducer<string, Proto.V2.TaskCreatedProto> _producerTaskCreatedV2;
    private readonly IKafkaDependentProducer<string, TaskAssignedProto> _producerTaskAssigned;
    private readonly IKafkaDependentProducer<string, TaskCompletedProto> _producerTaskCompleted;
    private readonly ILogger<TaskController> _logger;
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;

    public TaskController(
        IKafkaDependentProducer<string, TaskCreatedProto> producerTaskCreatedV1,
        IKafkaDependentProducer<string, Proto.V2.TaskCreatedProto> producerTaskCreatedV2,
        IKafkaDependentProducer<string, TaskAssignedProto> producerTaskAssigned,
        IKafkaDependentProducer<string, TaskCompletedProto> producerTaskCompleted,
        ILogger<TaskController> logger,
        ITaskRepository taskRepository,
        IUserRepository userRepository)
    {
        _producerTaskCreatedV1 = producerTaskCreatedV1;
        _producerTaskCreatedV2 = producerTaskCreatedV2;
        _producerTaskAssigned = producerTaskAssigned;
        _producerTaskCompleted = producerTaskCompleted;
        _logger = logger;
        _taskRepository = taskRepository;
        _userRepository = userRepository;
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

        var taskDto = await _taskRepository.Create(model, poPugId, cancellationToken);
        if (taskDto == null)
            return BadRequest("task not created");
        
        var valueV1 = new TaskCreatedProto
        {
            Base = new BaseProto
            {
                EventId = Guid.NewGuid().ToString("N"),
                EventName = Constants.KafkaEvent.TaskCreated,
                EventTime = DateTime.UtcNow.ToString("u"),
                EventVersion = "1"
            },
            PublicId = taskDto.Ulid,
            Title = model.Title,
            PoPugId = poPugId,
            Time = taskDto.CreatedAt.Ticks
        };

        try
        {
            await _producerTaskCreatedV1.ProduceAsync(
                Constants.KafkaTopic.TaskStreaming,
                new Message<string, TaskCreatedProto> { Key = userName, Value = valueV1 }
            );
        }
        catch (Exception ex)
        {
            // Add Error to DB
        }

        var valueV2 = new Proto.V2.TaskCreatedProto
        {
            Base = new BaseProto
            {
                EventId = Guid.NewGuid().ToString("N"),
                EventName = Constants.KafkaEvent.TaskCreated,
                EventTime = DateTime.UtcNow.ToString("u"),
                EventVersion = "2"
            },
            PublicId = taskDto.Ulid,
            Title = model.Title,
            JiraId = model.JiraId,
            PoPugId = poPugId,
            Time = taskDto.CreatedAt.Ticks
        };

        try
        {
            await _producerTaskCreatedV2.ProduceAsync(
                Constants.KafkaTopic.TaskStreaming,
                new Message<string, Proto.V2.TaskCreatedProto> { Key = userName, Value = valueV2 }
            );
        }
        catch (Exception ex)
        {
            // Add Error to DB
        }

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
            Status = (int)TaskMutationEnum.Completed,
            Time = DateTime.UtcNow.Ticks
        };
        
        try {
            await _producerTaskCompleted.ProduceAsync(
                Constants.KafkaTopic.TaskPropertiesMutation,
                new Message<string, TaskCompletedProto> { Key = userName, Value = value }
            );
        }
        catch (Exception ex)
        {
            // Add Error to DB
        }
        return Ok();
    }
    
    private async Task AssignTask(string identityName, TaskDto task, CancellationToken cancellationToken)
    {
        var value = new TaskAssignedProto
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
            Status = (int)TaskMutationEnum.Assign,
            Time = DateTime.UtcNow.Ticks
        };
        
        try {
            await _producerTaskAssigned.ProduceAsync(
                Constants.KafkaTopic.TaskPropertiesMutation,
                new Message<string, TaskAssignedProto> { Key = identityName, Value = value }
            );
        }
        catch (Exception ex)
        {
            // Add Error to DB
        }
    }
    
    private async Task AssignTasks(string identityName, TaskDto[] tasks, CancellationToken cancellationToken)
    {
        var messages = tasks.Select(task => new Message<string, TaskAssignedProto>
        {
            Key = identityName, Value =
                new TaskAssignedProto
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
                    Status = (int)TaskMutationEnum.Assign,
                    Time = DateTime.UtcNow.Ticks
                }
        }).ToList();
        
        // copy from https://github.com/confluentinc/confluent-kafka-dotnet/issues/890
        await _producerTaskAssigned.ProduceBatch(Constants.KafkaTopic.TaskPropertiesMutation, messages);

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
}
