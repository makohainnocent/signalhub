using FluentMigrator;

namespace DataAccess.Migrations
{
    [Migration(2023102512)]
    public class Migration_2023102512_CreateInspectionsTable : Migration
    {
        public override void Up()
        {
            // Create the Inspections table
            Create.Table("Inspections")
                .WithColumn("InspectionId").AsInt32().PrimaryKey().Identity() // Auto-incrementing primary key
                .WithColumn("InspectorId").AsInt32()
                .WithColumn("EntityId").AsInt32().NotNullable() // ID of the entity being inspected
                .WithColumn("InspectionType").AsString(50).NotNullable() // Type of entity being inspected (e.g., Premise, Product, Transfer, Transportation)
                .WithColumn("InspectionDate").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime) // Date of inspection
                .WithColumn("Status").AsString(50).NotNullable() // Inspection status (e.g., Pass, Fail, Pending)
                .WithColumn("Comments").AsString(500).Nullable() // Optional comments from the inspector
                .WithColumn("InspectionReportPdfBase64").AsString(int.MaxValue).Nullable() // Base64-encoded PDF of the inspection report
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime) // Timestamp when the inspection was created
                .WithColumn("UpdatedAt").AsDateTime().Nullable(); // Optional last update timestamp
        }

        public override void Down()
        {
            // Drop the Inspections table
            Delete.Table("Inspections");
        }
    }
}