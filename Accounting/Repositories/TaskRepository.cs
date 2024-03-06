using Accounting.Models;
using Core;
using Core.EventModels;

namespace Accounting.Repositories;

public class TaskRepository : GenericRepository<TaskDto>, ITaskRepository
{
    public TaskRepository(DapperContext context) : base(context)
    {
       
    }

    public async Task<TaskDto> Create(TaskCreatedEventModel model, CancellationToken cancellationToken)
    {
        var taskDto = new TaskDto
        {
            Ulid = model.PublicId,
            CreatedAt = DateTime.UtcNow,
            EditedAt = DateTime.UtcNow,
            Description = model.Description,
            PoPugId = model.PoPugId
        };
        var taskAdd = await Add(taskDto, cancellationToken);
        return taskAdd ? taskDto : throw new Exception("Task not created");
    }
}