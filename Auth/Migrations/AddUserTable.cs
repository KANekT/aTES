using Auth.Models;
using FluentMigrator;

namespace Auth.Migrations;

[Migration(1)]
public class AddUserTable : Migration
{
    public override void Up()
    {
        Create.Table("Users")
            .WithColumn(nameof(UserDto.Id)).AsInt64().PrimaryKey().Identity()
            .WithColumn(nameof(UserDto.Ulid)).AsString().NotNullable()
            .WithColumn(nameof(UserDto.CreatedAt)).AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn(nameof(UserDto.Login)).AsString().NotNullable()
            .WithColumn(nameof(UserDto.UserName)).AsString().NotNullable()
            .WithColumn(nameof(UserDto.Role)).AsByte()
            ;
        
        /*
        for (var i = 1; i < 25; i++)
        {
            var user = new Dictionary<string, object>
            {
                { nameof(UserDto.Login), "poPug_" + Random.Shared.Next(0, 500) },
                { nameof(UserDto.UserName), "PoPug # " + Random.Shared.Next(0, 500) },
                { nameof(UserDto.Role), (byte)RoleEnum.PoPug }
            };
            
            Insert.IntoTable("Users").Row(user);
        }
        
        for (var i = 1; i < 5; i++)
        {
            var user = new Dictionary<string, object>
            {
                { nameof(UserDto.Login), "manager_" + Random.Shared.Next(0, 500) },
                { nameof(UserDto.UserName), "Manager # " + Random.Shared.Next(0, 500) },
                { nameof(UserDto.Role), (byte)RoleEnum.Manager }
            };
            
            Insert.IntoTable("Users").Row(user);
        }
        
        var owner = new Dictionary<string, object>
        {
            { nameof(UserDto.Login), "owner"},
            { nameof(UserDto.UserName), "Owner CTO" },
            { nameof(UserDto.Role), (byte)RoleEnum.Admin }
        };
        Insert.IntoTable("Users").Row(owner);
        */
    }

    public override void Down()
    {
        Delete.Table("Users");
    }
}