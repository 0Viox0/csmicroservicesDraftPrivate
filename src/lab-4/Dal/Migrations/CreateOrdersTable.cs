using FluentMigrator;

namespace Dal.Migrations;

[Migration(2, "create orders table")]
public class CreateOrdersTable : Migration
{
    public override void Up()
    {
        Execute.Sql("CREATE TYPE order_state AS ENUM ('created', 'processing', 'completed', 'cancelled');");

        Create.Table("orders")
            .WithColumn("order_id").AsInt64().PrimaryKey().Identity()
            .WithColumn("order_state").AsCustom("order_state").NotNullable()
            .WithColumn("order_created_at").AsDateTimeOffset().NotNullable()
            .WithColumn("order_created_by").AsString().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("orders");
        Execute.Sql("DROP TYPE order_state");
    }
}