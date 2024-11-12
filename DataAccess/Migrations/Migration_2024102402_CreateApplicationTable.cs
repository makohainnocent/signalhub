using FluentMigrator;

namespace DataAccess.Migrations
{
    [Migration(2024102402)]
    public class Migration_2024102402_CreateApplicationTable : Migration
    {
        public override void Up()
        {
            Create.Table("Application")
                .WithColumn("ApplicationId").AsInt32().PrimaryKey().Identity() // Auto-incrementing primary key
                .WithColumn("FarmId").AsInt32().Nullable() // Foreign key to the farm
                .WithColumn("UserId").AsInt32().Nullable() // Foreign key to the user
                .WithColumn("Type").AsString(255).Nullable() // Type of application
                .WithColumn("RequestObject").AsCustom("NVARCHAR(MAX)").Nullable() // Large JSON or serialized request object
                .WithColumn("RequestDescription").AsString(255).Nullable()
                .WithColumn("Status").AsString(255).Nullable() // Status of the application
                .WithColumn("ResponseObject").AsCustom("NVARCHAR(MAX)").Nullable() // Large JSON or serialized response object
                .WithColumn("ResponseDescription").AsString(255).Nullable()
                .WithColumn("CreatedAt").AsDateTime().Nullable().WithDefault(SystemMethods.CurrentUTCDateTime) // Timestamp for creation
                .WithColumn("UpdatedAt").AsDateTime().Nullable().WithDefault(SystemMethods.CurrentUTCDateTime); // Timestamp for updates
        }

        public override void Down()
        {
            Delete.Table("Application");
        }
    }
}
