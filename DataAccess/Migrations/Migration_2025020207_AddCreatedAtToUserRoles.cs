using FluentMigrator;

namespace DataAccess.Migrations
{
    [Migration(2025020207)] // Use a unique timestamp-based migration ID
    public class Migration_2025020207_AddCreatedAtToUserRoles : Migration
    {
        public override void Up()
        {
            // Add the CreatedAt column
            Alter.Table("UserRoles")
                .AddColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);
        }

        public override void Down()
        {
            // Remove the CreatedAt column if rolling back
            Delete.Column("CreatedAt").FromTable("UserRoles");
        }
    }
}
