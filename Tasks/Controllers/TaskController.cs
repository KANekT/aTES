using Confluent.Kafka;
using Core;
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

        var task = await _taskRepository.Create(model, cancellationToken);
        if (task == null)
            return BadRequest("task not created");
        
        var taskCreated = new TaskCreatedEventModel
        {
            PublicId = task.Ulid,
            Description = model.Description,
            Lose = task.Lose,
            Reward = task.Reward
        }.Decode();
            
        _producer.Produce(
            Constants.KafkaTopic.TaskCreatedStream,
            new Message<string, string> { Key = userName, Value = taskCreated },
            _deliveryReportHandler
        );

        await AssignTask(userName, task, cancellationToken);

        return Ok();
    }

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
            await AssignTask(userName, task, cancellationToken);
        }
        
        return Ok();
    }

    [HttpPut("[action]/{id}")]
    public async Task<IActionResult> Completed(long id, CancellationToken cancellationToken)
    {
        var userName = CheckUser();
        if (string.IsNullOrEmpty(userName))
        {
            return Unauthorized();
        }

        var taskPublicId = await _taskRepository.Completed(id, userName, cancellationToken);

        _producer.Produce(
            Constants.KafkaTopic.TaskCompleted,
            new Message<string, string> { Key = userName, Value = taskPublicId },
            _deliveryReportHandler
        );

        return Ok();
    }

    private async Task AssignTask(string identityName, TaskDto task, CancellationToken cancellationToken)
    {
        var poPugId = await _userRepository.GetRandomPoPugId(cancellationToken);
        
        await _taskRepository.Assign(task.Id, poPugId, cancellationToken);
        
        var taskAssigned = new TaskAssignedEventModel
        {
            PublicTaskId = task.Ulid,
            PublicPoPugId = poPugId
        }.Decode();
        
        _producer.Produce(
            Constants.KafkaTopic.TaskAssigned,
            new Message<string, string> { Key = identityName, Value = taskAssigned },
            _deliveryReportHandler
        );
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
