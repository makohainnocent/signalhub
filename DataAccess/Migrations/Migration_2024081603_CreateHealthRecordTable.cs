using FluentMigrator;

namespace DataAccess.Migrations
{
    [Migration(2024081603)]
    public class Migration_2024081603_CreateHealthRecordTable : Migration
    {
        public override void Up()
        {
            // Create the HealthRecord table
            Create.Table("HealthRecord")
                .WithColumn("HealthRecordId").AsInt32().PrimaryKey().Identity() // Auto-incrementing primary key
                .WithColumn("AnimalId").AsInt32().NotNullable() // Link to livestock
                .WithColumn("UserId").AsInt32() // Link to user (veterinarian or inspector)
                .WithColumn("DateOfVisit").AsDateTime().NotNullable() // Date of the health visit
                .WithColumn("Diagnosis").AsString().Nullable() // Diagnosis details
                .WithColumn("Treatment").AsString().Nullable() // Treatment details
                .WithColumn("FollowUpDate").AsDateTime().Nullable() // Follow-up date (optional)
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime) // Timestamp when the record was created
                .WithColumn("UpdatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime); // Timestamp when the record was last updated

          
        }

        public override void Down()
        {
         

            // Drop the HealthRecord table
            Delete.Table("HealthRecord");
        }
    }
}