using FluentMigrator;

namespace Task3.Dal.Migrations;

[Migration(4, "create order history table")]
public class CreateOrderHistoryTable : Migration
{
    public override void Up()
    {
        Execute.Sql("CREATE TYPE order_history_item_kind AS ENUM ('created', 'item_added', 'item_removed', 'state_changed');");

        Create.Table("order_history")
            .WithColumn("order_history_item_id").AsInt64().PrimaryKey().Identity()
            .WithColumn("order_id").AsInt64().NotNullable().ForeignKey("orders", "order_id").OnDelete(System.Data.Rule.Cascade)
            .WithColumn("order_history_item_created_at").AsDateTime().NotNullable()
            .WithColumn("order_history_item_kind").AsCustom("order_history_item_kind").NotNullable()
            .WithColumn("order_history_item_payload").AsCustom("jsonb").NotNullable();
    }

    public override void Down()
    {
        Delete.Table("order_history");
        Execute.Sql("DROP TYPE IF EXISTS order_history_item_kind;");
    }
}