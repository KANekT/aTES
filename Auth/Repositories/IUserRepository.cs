using Auth.Models;
using Core;
using Core.Enums;

namespace Auth.Repositories;

public interface IUserRepository: IGenericRepository<UserDto>
{
    public Task<UserDto?> GetUser(string login, CancellationToken cancellationToken);
    public Task<UserDto?> Create(SignUpFormModel model, CancellationToken cancellationToken);
    public Task UpdateRole(string publicId, RoleEnum role, CancellationToken cancellationToken);
}