using FluentMigrator.Runner.VersionTableInfo;

namespace Tasks.Migrations;

[VersionTableMetaData]
public class CustomMetadataTable : DefaultVersionTableMetaData
{
    public override string SchemaName => "Tasks";
}