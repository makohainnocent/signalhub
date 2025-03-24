using FluentMigrator;

namespace DataAccess.Migrations
{
    [Migration(2025102406)]
    public class Migration_2025102406_CreateTagIssuanceTable : Migration
    {
        public override void Up()
        {
            Create.Table("TagIssuance")
                .WithColumn("IssuanceId").AsInt32().PrimaryKey().Identity() // Auto-incrementing primary key
                .WithColumn("TagId").AsInt32().NotNullable() // Foreign key to Tags table
                .WithColumn("ApplicationId").AsInt32().NotNullable() // Foreign key to PermitApplications table
                .WithColumn("IssuedToType").AsString(20).NotNullable() // Type of entity the tag is issued to (Farmer, Farm, Other)
                .WithColumn("IssuedToId").AsInt32().NotNullable() // ID of the entity the tag is issued to
                .WithColumn("IssuedBy").AsInt32().NotNullable() // User who issued the tag (links to Users table)
                .WithColumn("IssueDate").AsDateTime().NotNullable() // Date the tag was issued
                .WithColumn("ExpiryDate").AsDateTime().Nullable() // Optional expiry date
                .WithColumn("Status").AsString(20).NotNullable().WithDefaultValue("Active") // Current status of the tag (Active, Revoked, Lost, Damaged)
                .WithColumn("RevokedAt").AsDateTime().Nullable() // Timestamp when the tag was revoked
                .WithColumn("RevokedBy").AsInt32().Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime) // Timestamp when the issuance record was created
                .WithColumn("UpdatedAt").AsDateTime().Nullable(); // Optional last update timestamp
        }

        public override void Down()
        {
            Delete.Table("TagIssuance");
        }
    }
}