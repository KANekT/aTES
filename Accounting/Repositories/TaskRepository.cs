using Accounting.Models;
using Core;

namespace Accounting.Repositories;

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
}