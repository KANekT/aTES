using Analytics.Models;
using Core.Enums;
using FluentMigrator;

namespace Analytics.Migrations;

[Migration(2)]
public class AddTaskTable : Migration
{
    public override void Up()
    {
        Create.Table("Tasks")
            .WithColumn(nameof(TaskDto.Id)).AsInt64().PrimaryKey().Identity()
            .WithColumn(nameof(TaskDto.Ulid)).AsString().NotNullable()
            .WithColumn(nameof(TaskDto.CreatedAt)).AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn(nameof(TaskDto.EditedAt)).AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn(nameof(TaskDto.Title)).AsString().NotNullable()
            .WithColumn(nameof(TaskDto.Status)).AsByte().NotNullable().WithDefaultValue((byte)TaskStatusEnum.Open)
            .WithColumn(nameof(TaskDto.Lose)).AsDecimal().Nullable()
            .WithColumn(nameof(TaskDto.Reward)).AsDecimal().Nullable()
            .WithColumn(nameof(TaskDto.PoPugId)).AsString().NotNullable()
            ;
    }

    public override void Down()
    {
        Delete.Table("Tasks");
    }
}