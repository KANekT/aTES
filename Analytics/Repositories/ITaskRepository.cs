using Analytics.Models;
using Core;

namespace Analytics.Repositories;

public interface ITaskRepository: IGenericRepository<TaskDto>
{
    public Task<TaskDto> Create(TaskDto model, CancellationToken cancellationToken);
    public Task<decimal> MostExpensiveTask(DateTime start, DateTime end, CancellationToken cancellationToken);
}