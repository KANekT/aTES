using Accounting.Models;
using Core;

namespace Accounting.Repositories;

public interface ITaskRepository: IGenericRepository<TaskDto>
{
    public Task<TaskDto> Create(long time, string publicId, CancellationToken cancellationToken);
    public Task<TaskDto> Create(long time, string publicId, string title, CancellationToken cancellationToken);
    public Task<TaskDto> Create(long time, string publicId, string title, string jiraId, CancellationToken cancellationToken);
    public Task<TaskDto[]> GetAllClosed(CancellationToken cancellationToken);
}