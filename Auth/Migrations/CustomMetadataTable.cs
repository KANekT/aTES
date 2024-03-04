using FluentMigrator.Runner.VersionTableInfo;

namespace Auth.Migrations;

[VersionTableMetaData]
public class CustomMetadataTable : DefaultVersionTableMetaData
{
    public override string SchemaName => "Auth";
}