using Auth.Models;
using Core;
using Core.Enums;

namespace Auth.Repositories;

public class UserRepository : GenericRepository<UserDto>, IUserRepository
{
    public UserRepository(DapperContext context) : base(context)
    {
       
    }

    public async Task<UserDto?> GetUser(string login, CancellationToken cancellationToken)
    {
        var users = await GetAll(cancellationToken);
        return users.FirstOrDefault(x => x.Login == login);
    }

    public async Task<UserDto?> Create(SignUpFormModel model, CancellationToken cancellationToken)
    {
        var userDto = new UserDto
        {
            Ulid = Ulid.NewUlid().ToString(),
            CreatedAt = DateTime.UtcNow,
            Login = model.Login,
            UserName = model.UserName,
            Role = model.Role
        };
        var userAdd = await Add(userDto, cancellationToken);
        return userAdd ? userDto : null;
    }

    public async Task UpdateRole(string publicId, RoleEnum role, CancellationToken cancellationToken)
    {
        var user = await GetByPublicId(publicId, cancellationToken);
        if (user != null)
        {
            user.Role = role;
            await Update(user, cancellationToken);
        }
    }
}