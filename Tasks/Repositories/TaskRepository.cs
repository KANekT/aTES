using Core;
using Core.Enums;
using Tasks.Models;

namespace Tasks.Repositories;

public class TaskRepository : GenericRepository<TaskDto>, ITaskRepository
{
    public TaskRepository(DapperContext context) : base(context)
    {
       
    }

    public async Task<TaskDto?> Create(TaskCreateFormModel model, string poPugId, CancellationToken cancellationToken)
    {
        var taskDto = new TaskDto
        {
            CreatedAt = DateTime.UtcNow,
            EditedAt = DateTime.UtcNow,
            Description = model.Description,
            PoPugId = poPugId
        };
        var taskAdd = await Add(taskDto, cancellationToken);
        return taskAdd ? taskDto : null;
    }

    public async Task<string> Completed(long id, string userPublicId, CancellationToken cancellationToken)
    {
        var tasks = await GetAll(cancellationToken);
        var task = tasks.FirstOrDefault(x => x.Id == id);
        if (task == null)
        {
            throw new Exception("task is not exits");
        }
        if (task.PoPugId != userPublicId)
        {
            throw new Exception("task is not4you");
        }
        
        task.Status = StatusEnum.Completed;
        task.EditedAt = DateTime.UtcNow;
        
        await Update(task, cancellationToken);

        return task.Ulid;
    }

    public async Task Assign(long id, string userPublicId, CancellationToken cancellationToken)
    {
        var tasks = await GetAll(cancellationToken);
        var task = tasks.FirstOrDefault(x => x.Id == id);
        if (task == null)
        {
            throw new Exception("task is not exits");
        }

        task.PoPugId = userPublicId;
        task.EditedAt = DateTime.UtcNow;
        
        await Update(task, cancellationToken);
    }

    public async Task<TaskDto[]> GetAllOpen(CancellationToken cancellationToken)
    {
        var tasks = await GetAll(cancellationToken);

        return tasks.Where(x => x.Status == StatusEnum.Open).ToArray();
    }

    public async Task<TaskDto[]> My(string userPublicId, CancellationToken cancellationToken)
    {
        var tasks = await GetAll(cancellationToken);

        return tasks.Where(x => x.PoPugId == userPublicId).ToArray();
    }
}