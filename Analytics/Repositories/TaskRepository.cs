using Analytics.Models;
using Core;
using Core.Enums;

namespace Analytics.Repositories;

public class TaskRepository : GenericRepository<TaskDto>, ITaskRepository
{
    public TaskRepository(DapperContext context) : base(context)
    {
       
    }

    public async Task<TaskDto> Create(TaskDto taskDto, CancellationToken cancellationToken)
    {
        var taskAdd = await Add(taskDto, cancellationToken);
        return taskAdd ? taskDto : throw new Exception("Task not created");
    }

    public async Task<TaskDto[]> GetAllClosed(CancellationToken cancellationToken)
    {
        var tasks = await GetAll(cancellationToken);
        return tasks.Where(x => x.Status == TaskStatusEnum.Completed).ToArray();
    }
    
    public async Task<decimal> MostExpensiveTask(DateTime start, DateTime end, CancellationToken cancellationToken)
    {
        var tasks = await GetAll(cancellationToken);
        var closed = tasks.Where(x =>
            x.EditedAt.Date >= start
            && x.EditedAt.Date <= end
            && x.Status == TaskStatusEnum.Completed
        );
        return closed.Max(t => t.Reward ?? 0);
    }
}