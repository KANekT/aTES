using Accounting.Models;
using Core;

namespace Accounting.Repositories;

public interface ITaskRepository: IGenericRepository<TaskDto>
{
    public Task<TaskDto> Create(TaskDto model, CancellationToken cancellationToken);
    public Task<TaskDto[]> GetAllClosed(CancellationToken cancellationToken);
}