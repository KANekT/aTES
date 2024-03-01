using Core.Enums;
using FluentMigrator;
using Tasks.Models;

namespace Tasks.Migrations;

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
            .WithColumn(nameof(TaskDto.Description)).AsString().NotNullable()
            .WithColumn(nameof(TaskDto.Status)).AsByte().NotNullable().WithDefaultValue((byte)StatusEnum.Open)
            .WithColumn(nameof(TaskDto.Lose)).AsInt32().NotNullable()
            .WithColumn(nameof(TaskDto.Reward)).AsInt32().NotNullable()
            .WithColumn(nameof(TaskDto.PoPugId)).AsString().NotNullable()
            ;

        /*
        for (var i = 0; i < 50; i++)
        {
            var task = new Dictionary<string, object>
            {
                { nameof(TaskDto.Description), "Task # " + Random.Shared.Next(0, 500) },
                { nameof(TaskDto.Lose), Random.Shared.Next(10, 20) },
                { nameof(TaskDto.Reward), Random.Shared.Next(20, 40) },
                { nameof(TaskDto.PoPugId), Random.Shared.Next(1, 25) }
            };
            
            Insert.IntoTable(nameof(Task)).Row(task);
        }
        */
    }

    public override void Down()
    {
        Delete.Table(nameof(TaskDto));
    }
}