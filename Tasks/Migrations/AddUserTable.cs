using FluentMigrator;
using Tasks.Models;

namespace Tasks.Migrations;

[Migration(1)]
public class AddUserTable : Migration
{
    public override void Up()
    {
        Create.Table("Users")
            .WithColumn(nameof(UserDto.Id)).AsInt64().PrimaryKey().Identity()
            .WithColumn(nameof(UserDto.Ulid)).AsString().NotNullable()
            .WithColumn(nameof(UserDto.Role)).AsByte()
            ;
    }

    public override void Down()
    {
        Delete.Table("Users");
    }
}