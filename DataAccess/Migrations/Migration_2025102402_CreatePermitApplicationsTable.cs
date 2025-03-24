using FluentMigrator;

namespace DataAccess.Migrations
{
    [Migration(2025102402)]
    public class Migration_2025102402_CreatePermitApplicationsTable : Migration
    {
        public override void Up()
        {
            Create.Table("PermitApplications")
                .WithColumn("ApplicationId").AsInt32().PrimaryKey().Identity() // Auto-incrementing primary key
                .WithColumn("PermitId").AsInt32()
                .WithColumn("ApplicantType").AsString(20).NotNullable() // Type of applicant (e.g., Farmer, Farm, Transporter)
                .WithColumn("ApplicantId").AsInt32().NotNullable() // ID of the applicant
                .WithColumn("Documents").AsString(int.MaxValue).NotNullable() // Base64-encoded PDF of required documents
                .WithColumn("Status").AsString(20).NotNullable() // Status of the application (e.g., Pending, Approved, Rejected, Revoked)
                .WithColumn("AppliedBy").AsInt32().NotNullable()
                .WithColumn("AppliedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime) // Timestamp when the application was submitted
                .WithColumn("ReviewedBy").AsInt32().Nullable()
                .WithColumn("ReviewedAt").AsDateTime().Nullable() // Timestamp when the application was reviewed
                .WithColumn("IssuedAt").AsDateTime().Nullable() // Timestamp when the permit was issued
                .WithColumn("ExpiryDate").AsDateTime().Nullable() // Expiry date of the permit
                .WithColumn("PermitPdf").AsString(int.MaxValue).Nullable() // Base64-encoded PDF of the issued permit
                .WithColumn("RevokedAt").AsDateTime().Nullable() // Timestamp when the permit was revoked
                .WithColumn("RevokedBy").AsInt32()
                .WithColumn("Comments").AsString(500).Nullable(); // Comments or notes from the reviewer
        }

        public override void Down()
        {
            Delete.Table("PermitApplications");
        }
    }
}