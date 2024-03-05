using Accounting.Models;
using Core;
using Core.EventModels;

namespace Accounting.Repositories;

public interface ITaskRepository: IGenericRepository<TaskDto>
{
    public Task<TaskDto?> Create(TaskCreatedEventModel model, CancellationToken cancellationToken);
}