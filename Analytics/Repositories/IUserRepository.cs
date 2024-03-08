using Analytics.Models;
using Core;
using Core.Enums;

namespace Analytics.Repositories;

public interface IUserRepository: IGenericRepository<UserDto>
{
    public Task<UserDto?> Create(string publicId, RoleEnum role, CancellationToken cancellationToken);
    public Task RoleChange(string publicId, RoleEnum role, CancellationToken cancellationToken);
}