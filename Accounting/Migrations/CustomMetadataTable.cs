using FluentMigrator.Runner.VersionTableInfo;

namespace Accounting.Migrations;

[VersionTableMetaData]
public class CustomMetadataTable : DefaultVersionTableMetaData
{
    public override string SchemaName => "Accounting";
}