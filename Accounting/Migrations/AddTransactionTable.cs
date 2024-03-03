using Accounting.Models;
using FluentMigrator;

namespace Accounting.Migrations;

[Migration(3)]
public class AddTransactionTable : Migration
{
    public override void Up()
    {
        Create.Table("Transactions")
            .WithColumn(nameof(TransactionDto.Id)).AsInt64().PrimaryKey().Identity()
            .WithColumn(nameof(TransactionDto.Ulid)).AsString().NotNullable()
            .WithColumn(nameof(TransactionDto.CreatedAt)).AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn(nameof(TransactionDto.Type)).AsByte().NotNullable()
            .WithColumn(nameof(TransactionDto.PoPugId)).AsString().NotNullable()
            .WithColumn(nameof(TransactionDto.Money)).AsCurrency().NotNullable()
            ;
    }

    public override void Down()
    {
        Delete.Table("Transactions");
    }
}