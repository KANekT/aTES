using Accounting.Models;
using Core;
using Core.Enums;

namespace Accounting.Repositories;

public interface IUserRepository: IGenericRepository<UserDto>
{
    public Task<UserDto> Create(string publicId, CancellationToken cancellationToken);
    public Task<UserDto> Create(string publicId, RoleEnum role, CancellationToken cancellationToken);
}