using Accounting.Models;
using FluentMigrator;

namespace Accounting.Migrations;

[Migration(1)]
public class AddUserTable : Migration
{
    public override void Up()
    {
        Create.Table("Users")
            .WithColumn(nameof(UserDto.Id)).AsInt64().PrimaryKey().Identity()
            .WithColumn(nameof(UserDto.Ulid)).AsString().NotNullable()
            .WithColumn(nameof(UserDto.Role)).AsByte()
            .WithColumn(nameof(UserDto.Balance)).AsCurrency()
            ;
    }

    public override void Down()
    {
        Delete.Table("Users");
    }
}