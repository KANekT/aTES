using Accounting.Models;
using Core;
using Core.Enums;

namespace Accounting.Repositories;

public class UserRepository : GenericRepository<UserDto>, IUserRepository
{
    public UserRepository(DapperContext context) : base(context)
    {
       
    }

    public async Task<UserDto> Create(string publicId, CancellationToken cancellationToken)
    {
        var userDto = new UserDto
        {
            Ulid = publicId,
            Role = RoleEnum.Default
        };
        var userAdd = await Add(userDto, cancellationToken);
        return userAdd ? userDto : throw new Exception("user is not created");
    }

    public async Task<UserDto?> Create(string publicId, RoleEnum role, CancellationToken cancellationToken)
    {
        var user = await GetByPublicId(publicId, cancellationToken);
        if (user != null)
        {
            throw new Exception("user is exits");
        }

        var userDto = new UserDto
        {
            Ulid = publicId,
            Role = role
        };
        var userAdd = await Add(userDto, cancellationToken);
        return userAdd ? userDto : null;
    }

    public async Task RoleChange(string publicId, RoleEnum role, CancellationToken cancellationToken)
    {
        var user = await GetByPublicId(publicId, cancellationToken);
        if (user == null)
        {
            throw new Exception("user is not exits");
        }
        
        user.Role = role;
        await Update(user, cancellationToken);
    }

    public async Task UpdateBalance(string publicId, decimal money, CancellationToken cancellationToken)
    {
        var user = await GetByPublicId(publicId, cancellationToken);
        if (user == null)
        {
            throw new Exception("user is not exits");
        }

        user.Balance += money;
        await Update(user, cancellationToken);
    }
}