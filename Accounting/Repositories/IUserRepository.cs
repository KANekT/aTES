using Accounting.Models;
using Core.Enums;

namespace Accounting.Repositories;

public interface IUserRepository
{
    public Task<UserDto?> Create(string publicId, RoleEnum role, CancellationToken cancellationToken);
    public Task RoleChange(string publicId, RoleEnum role, CancellationToken cancellationToken);
}