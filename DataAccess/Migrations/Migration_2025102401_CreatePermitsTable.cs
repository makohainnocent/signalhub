using FluentMigrator;

namespace DataAccess.Migrations
{
    [Migration(2025102401)]
    public class Migration_2025102401_CreatePermitsTable : Migration
    {
        public override void Up()
        {
            Create.Table("Permits")
                .WithColumn("PermitId").AsInt32().PrimaryKey().Identity() // Auto-incrementing primary key
                .WithColumn("PermitName").AsString(100).NotNullable() // Name of the permit
                .WithColumn("Description").AsString(500).Nullable() // Description of the permit
                .WithColumn("Requirements").AsString(500).Nullable() // Requirements for the permit
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime) // Timestamp when the permit was created
                .WithColumn("UpdatedAt").AsDateTime().Nullable(); // Optional last update timestamp
        }

        public override void Down()
        {
            Delete.Table("Permits");
        }
    }
}