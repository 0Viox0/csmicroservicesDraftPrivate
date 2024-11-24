using FluentMigrator;

namespace Dal.Migrations;

[Migration(3, "create order items table")]
public class CreateOrderItemsTable : Migration
{
    public override void Up()
    {
        Create.Table("order_items")
            .WithColumn("order_item_id").AsInt64().PrimaryKey().Identity()
            .WithColumn("order_id").AsInt64().NotNullable().ForeignKey("orders", "order_id")
            .WithColumn("product_id").AsInt64().NotNullable().ForeignKey("products", "product_id")
            .WithColumn("order_item_quantity").AsInt32().NotNullable()
            .WithColumn("order_item_deleted").AsBoolean().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("order_items");
    }
}