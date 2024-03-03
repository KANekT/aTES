using Accounting.Models;
using Core.EventModels;

namespace Accounting.Repositories;

public interface ITaskRepository
{
    public Task<TaskDto?> Create(TaskCreatedEventModel model, string poPugId, CancellationToken cancellationToken);
}