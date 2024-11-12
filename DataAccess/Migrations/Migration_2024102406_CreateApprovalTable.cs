using FluentMigrator;

namespace DataAccess.Migrations
{
    [Migration(2024102406)]
    public class Migration_2024102406_CreateApprovalTable : Migration
    {
        public override void Up()
        {
            Create.Table("Approval")
                .WithColumn("ApprovalId").AsInt32().PrimaryKey().Identity() // Auto-incrementing primary key
                .WithColumn("UserId").AsInt32().NotNullable() // ID of the user requesting or approving
                .WithColumn("FarmId").AsInt32().NotNullable() // ID of the farm associated with the approval
                .WithColumn("LivestockIds").AsString(int.MaxValue).NotNullable().WithDefaultValue(string.Empty) // List of livestock IDs, e.g., "1,2,3"
                .WithColumn("ApprovalDocument").AsCustom("nvarchar(max)").Nullable().WithDefaultValue(string.Empty) // Base64 encoded approval document
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime) // Creation timestamp
                .WithColumn("UpdatedAt").AsDateTime().Nullable().WithDefault(SystemMethods.CurrentUTCDateTime) // Last update timestamp
                .WithColumn("Notes").AsString(int.MaxValue).Nullable().WithDefaultValue(string.Empty); // Optional notes or remarks
        }

        public override void Down()
        {
            Delete.Table("Approval");
        }
    }
}
