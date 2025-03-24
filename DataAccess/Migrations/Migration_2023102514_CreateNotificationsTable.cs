using FluentMigrator;

namespace DataAccess.Migrations
{
    [Migration(2023102514)]
    public class Migration_2023102514_CreateNotificationsTable : Migration
    {
        public override void Up()
        {
            // Create the Notifications table
            Create.Table("Notifications")
                .WithColumn("NotificationId").AsInt32().PrimaryKey().Identity() // Auto-incrementing primary key
                .WithColumn("UserId").AsInt32() 
                .WithColumn("Title").AsString(100).NotNullable() 
                .WithColumn("Body").AsString(500).NotNullable() 
                .WithColumn("Status").AsString(50).NotNullable().WithDefaultValue("Pending") // Notification status (e.g., Pending, Sent, Failed)
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime) // Timestamp when the notification was created
                .WithColumn("SentAt").AsDateTime().Nullable(); // Timestamp when the notification was sent
        }

        public override void Down()
        {
            // Drop the Notifications table
            Delete.Table("Notifications");
        }
    }
}