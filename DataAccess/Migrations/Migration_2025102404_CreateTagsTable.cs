using FluentMigrator;

namespace DataAccess.Migrations
{
    [Migration(2025102404)]
    public class Migration_2025102404_CreateTagsTable : Migration
    {
        public override void Up()
        {
            Create.Table("Tags")
                .WithColumn("TagId").AsInt32().PrimaryKey().Identity() // Auto-incrementing primary key
                .WithColumn("TagNumber").AsString(50).NotNullable().Unique() // Unique tag number
                .WithColumn("TagType").AsString(20).NotNullable() // Type of tag (RFID, Ear Tag, Microchip)
                .WithColumn("Manufacturer").AsString(100).NotNullable() // Manufacturer of the tag
                .WithColumn("BatchNumber").AsString(50).Nullable() // Batch number for tracking
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime); // Timestamp when the tag was registered
        }

        public override void Down()
        {
            Delete.Table("Tags");
        }
    }
}