using FluentMigrator;

namespace DataAccess.Migrations
{
    [Migration(2023102513)]
    public class Migration_2023102513_CreateUserDevicesTable : Migration
    {
        public override void Up()
        {
            // Create the UserDevices table
            Create.Table("UserDevices")
                .WithColumn("DeviceId").AsInt32().PrimaryKey().Identity() // Auto-incrementing primary key
                .WithColumn("UserId").AsInt32()
                .WithColumn("DeviceToken").AsString(500).NotNullable() // FCM device token
                .WithColumn("Platform").AsString(50).NotNullable() // Platform (e.g., Android, iOS, Web)
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime) // Timestamp when the device token was registered
                .WithColumn("UpdatedAt").AsDateTime().Nullable(); // Optional last update timestamp
        }

        public override void Down()
        {
            // Drop the UserDevices table
            Delete.Table("UserDevices");
        }
    }
}