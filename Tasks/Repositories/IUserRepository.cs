using Core.Enums;
using Tasks.Models;

namespace Tasks.Repositories;

public interface IUserRepository
{
    public Task<UserDto?> Create(string publicId, RoleEnum role, CancellationToken cancellationToken);
    public Task RoleChange(string publicId, RoleEnum role, CancellationToken cancellationToken);
    public Task<string> GetRandomPoPugId(CancellationToken cancellationToken);
}