using Accounting.Models;
using Core;
using Core.Enums;

namespace Accounting.Repositories;

public class TaskRepository : GenericRepository<TaskDto>, ITaskRepository
{
    public TaskRepository(DapperContext context) : base(context)
    {
       
    }

    public async Task<TaskDto> Create(long time, string publicId, CancellationToken cancellationToken)
    {
        var taskDto = new TaskDto
        {
            CreatedAt = new DateTime(time),
            EditedAt = DateTime.UtcNow,
            Ulid = publicId,
            Title = string.Empty
        };
        var taskAdd = await Add(taskDto, cancellationToken);
        return taskAdd ? taskDto : throw new Exception("Task not created");
    }

    public async Task<TaskDto> Create(long time, string publicId, string title, CancellationToken cancellationToken)
    {
        var taskDto = new TaskDto
        {
            CreatedAt = new DateTime(time),
            EditedAt = DateTime.UtcNow,
            Ulid = publicId,
            Title = title
        };
        var taskAdd = await Add(taskDto, cancellationToken);
        return taskAdd ? taskDto : throw new Exception("Task not created");
    }

    public async Task<TaskDto> Create(long time, string publicId, string title, string jiraId, CancellationToken cancellationToken)
    {
        var taskDto = new TaskDto
        {
            CreatedAt = new DateTime(time),
            EditedAt = DateTime.UtcNow,
            Ulid = publicId,
            Title = title,
            JiraId = jiraId
        };
        var taskAdd = await Add(taskDto, cancellationToken);
        return taskAdd ? taskDto : throw new Exception("Task not created");
    }

    public async Task<TaskDto[]> GetAllClosed(CancellationToken cancellationToken)
    {
        var tasks = await GetAll(cancellationToken);
        return tasks.Where(x => x.Status == TaskStatusEnum.Completed).ToArray();
    }
}