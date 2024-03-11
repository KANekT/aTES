using Analytics.Models;
using FluentMigrator;

namespace Analytics.Migrations;

[Migration(4)]
public class AddTaskJiraIdColumn : Migration
{
    public override void Up()
    {
        Create.Column(nameof(TaskDto.JiraId)).OnTable("Tasks").AsString().Nullable();
    }

    public override void Down()
    {
        Delete.Column(nameof(TaskDto.JiraId)).FromTable("Tasks");
    }
}