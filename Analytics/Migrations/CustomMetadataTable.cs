using FluentMigrator.Runner.VersionTableInfo;

namespace Analytics.Migrations;

[VersionTableMetaData]
public class CustomMetadataTable : DefaultVersionTableMetaData
{
    public override string SchemaName => "Analytics";
}