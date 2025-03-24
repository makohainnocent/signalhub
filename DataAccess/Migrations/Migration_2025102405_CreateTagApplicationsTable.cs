using FluentMigrator;

namespace DataAccess.Migrations
{
    [Migration(2025102405)]
    public class Migration_2025102405_CreateTagApplicationsTable : Migration
    {
        public override void Up()
        {
            Create.Table("TagApplications")
                .WithColumn("ApplicationId").AsInt32().PrimaryKey().Identity() // Auto-incrementing primary key
                .WithColumn("ApplicantType").AsString(20).NotNullable() // Type of applicant (Farmer, Farm, Other)
                .WithColumn("ApplicantId").AsInt32().NotNullable() // ID of the applicant
                .WithColumn("NumberOfTags").AsInt32().NotNullable() // Number of tags requested
                .WithColumn("Purpose").AsString(200).Nullable() // Purpose of the tag request
                .WithColumn("Status").AsString(20).NotNullable() // Status of the application (Pending, Approved, Rejected)
                .WithColumn("AppliedBy").AsInt32().NotNullable() // User who submitted the application
                .WithColumn("AppliedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime) // Timestamp when the application was submitted
                .WithColumn("ReviewedBy").AsInt32().Nullable()// User who reviewed the application
                .WithColumn("ReviewedAt").AsDateTime().Nullable() // Timestamp when the application was reviewed
                .WithColumn("Comments").AsString(500).Nullable(); // Comments or notes from the reviewer
        }

        public override void Down()
        {
            Delete.Table("TagApplications");
        }
    }
}