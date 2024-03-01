using Core;
using Core.Enums;
using Tasks.Models;

namespace Tasks.Repositories;

public class UserRepository : GenericRepository<UserDto>, IUserRepository
{
    public UserRepository(DapperContext context) : base(context)
    {
       
    }

    public async Task<UserDto?> Create(string publicId, RoleEnum role, CancellationToken cancellationToken)
    {
        var users = await GetAll(cancellationToken);
        var user = users.FirstOrDefault(x => x.Ulid == publicId);
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
        var users = await GetAll(cancellationToken);
        var user = users.FirstOrDefault(x => x.Ulid == publicId);
        if (user == null)
        {
            throw new Exception("user is not exits");
        }
        
        user.Role = role;
        await Update(user, cancellationToken);
    }
    
    public async Task<string> GetRandomPoPugId(CancellationToken cancellationToken)
    {
        var users = await GetAll(cancellationToken);
        var poPugs = users.Where(x => x.Role != RoleEnum.Admin && x.Role != RoleEnum.Manager).ToArray();
        var random = new Random();
        var index = random.Next(0, poPugs.Length);

        return poPugs[index].Ulid;
    }
}