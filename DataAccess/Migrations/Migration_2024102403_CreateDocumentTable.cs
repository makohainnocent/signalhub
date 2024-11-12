using FluentMigrator;

namespace DataAccess.Migrations
{
    [Migration(2024102403)]
    public class Migration_2024102403_CreateDocumentTable : Migration
    {
        public override void Up()
        {
            Create.Table("Document")
                .WithColumn("DocumentId").AsInt32().PrimaryKey().Identity() // Auto-incrementing primary key
                .WithColumn("FarmId").AsInt32().Nullable() // Foreign key to the farm
                .WithColumn("UserId").AsInt32().Nullable() // Foreign key to the user
                .WithColumn("AnimalId").AsInt32().Nullable() // Foreign key to the animal
                .WithColumn("Type").AsString(255).Nullable() // Type of document
                .WithColumn("Owner").AsString(255).Nullable() // Document owner
                .WithColumn("Description").AsString(255).Nullable() // Document description
                .WithColumn("DocumentString").AsCustom("nvarchar(max)").Nullable().WithDefaultValue("[]") // Base64 encoded document string
                .WithColumn("CreatedAt").AsDateTime().Nullable().WithDefault(SystemMethods.CurrentUTCDateTime) // Timestamp for creation
                .WithColumn("UpdatedAt").AsDateTime().Nullable().WithDefault(SystemMethods.CurrentUTCDateTime); // Timestamp for updates
        }

        public override void Down()
        {
            Delete.Table("Document");
        }
    }
}
