using FluentMigrator;

namespace Task3.Dal.Migrations;

[Migration(1, "create product table")]
public class CreateProductTable : Migration
{
    public override void Up()
    {
        Create.Table("products")
            .WithColumn("product_id").AsInt64().PrimaryKey().Identity()
            .WithColumn("product_name").AsString().NotNullable()
            .WithColumn("product_price").AsDecimal().Nullable();
    }

    public override void Down()
    {
        Delete.Table("products");
    }
}