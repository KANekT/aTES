using Tasks.Models;

namespace Tasks.Repositories;

public interface ITaskRepository
{
    public Task<TaskDto?> Create(TaskCreateFormModel model, string poPugId, CancellationToken cancellationToken);
    public Task<string> Completed(long id, string userPublicId, CancellationToken cancellationToken);
    public Task Assign(long id, string userPublicId, CancellationToken cancellationToken);
    public Task<TaskDto[]> GetAllOpen(CancellationToken cancellationToken);
    public Task<TaskDto[]> My(string userPublicId, CancellationToken cancellationToken);
}