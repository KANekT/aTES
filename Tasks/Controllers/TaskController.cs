using Confluent.Kafka;
using Core;
using Core.Enums;
using Core.EventModels;
using Core.Extensions;
using Core.Kafka;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasks.Models;
using Tasks.Repositories;

namespace Tasks.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class TaskController : ControllerBase
{
    private readonly KafkaDependentProducer<string, string> _producer;
    private readonly ILogger<TaskController> _logger;
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;

    public TaskController(
        KafkaDependentProducer<string, string> producer,
        ILogger<TaskController> logger,
        ITaskRepository userRepository,
        IUserRepository userRepository1)
    {
        _producer = producer;
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
        
        var poPugId = await _userRepository.GetRandomPoPugId(cancellationToken);

        var task = await _taskRepository.Create(model, poPugId, cancellationToken);
        if (task == null)
            return BadRequest("task not created");
        
        var taskCreated = new TaskCreatedEventModel
        {
            PublicId = task.Ulid,
            Description = model.Description,
            PoPugId = poPugId,
        }.Decode();
            
        _producer.Produce(
            Constants.KafkaTopic.TaskCreatedStream,
            new Message<string, string> { Key = userName, Value = taskCreated },
            _deliveryReportHandler
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

        var eventModel = new TaskMutationEventModel
        {
            PublicTaskId = taskPublicId,
            PublicPoPugId = userName,
            Status = TaskMutationEnum.Completed
        }.Decode();
        
        _producer.Produce(
            Constants.KafkaTopic.TaskPropertiesMutation,
            new Message<string, string> { Key = userName, Value = eventModel },
            _deliveryReportHandler
        );

        return Ok();
    }

    private async Task AssignTask(string identityName, TaskDto task, CancellationToken cancellationToken)
    {
        var eventModel = new TaskMutationEventModel
        {
            PublicTaskId = task.Ulid,
            PublicPoPugId = task.PoPugId,
            Status = TaskMutationEnum.Assign
        }.Decode();
        
        _producer.Produce(
            Constants.KafkaTopic.TaskPropertiesMutation,
            new Message<string, string> { Key = identityName, Value = eventModel },
            _deliveryReportHandler
        );
    }
    
    private async Task AssignTasks(string identityName, TaskDto[] tasks, CancellationToken cancellationToken)
    {
        var messages = tasks.Select(task => new Message<string, string>
        {
            Key = identityName, Value = new TaskMutationEventModel
            {
                PublicTaskId = task.Ulid,
                PublicPoPugId = task.PoPugId,
                Status = TaskMutationEnum.Assign
            }.Decode()
        }).ToList();
        
        // copy from https://github.com/confluentinc/confluent-kafka-dotnet/issues/890
        _producer.ProduceBatch(Constants.KafkaTopic.TaskPropertiesMutation, messages);

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
    
    private void _deliveryReportHandler(DeliveryReport<string, string> deliveryReport)
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
