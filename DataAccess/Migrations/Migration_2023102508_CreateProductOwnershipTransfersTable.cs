using FluentMigrator;

namespace DataAccess.Migrations
{
    [Migration(2023102508)]
    public class Migration_2023102508_CreateProductOwnershipTransfersTable : Migration
    {
        public override void Up()
        {
            // Create the ProductOwnershipTransfers table
            Create.Table("ProductOwnershipTransfers")
                .WithColumn("TransferId").AsInt32().PrimaryKey().Identity() // Auto-incrementing primary key
                .WithColumn("ProductId").AsInt32().Nullable() // Link to product (nullable if product is not tracked)
                .WithColumn("ProductName").AsString(100).NotNullable() // Name of the product (required)
                .WithColumn("ProductType").AsString(50).NotNullable() // Type of the product (e.g., Meat, Dairy, Feed)
                .WithColumn("ProductDescription").AsString(500).Nullable() // Description of the product
                .WithColumn("FromPremiseId").AsInt32().Nullable() // Current owner (initiator premise, nullable if not tracked)
                .WithColumn("FromPremiseName").AsString(100).NotNullable() // Name of the initiator premise (required)
                .WithColumn("FromPremiseAddress").AsString(200).NotNullable() // Address of the initiator premise
                .WithColumn("ToPremiseId").AsInt32().Nullable() // New owner (recipient premise, nullable if external)
                .WithColumn("ToPremiseName").AsString(100).NotNullable() // Name of the recipient premise (required)
                .WithColumn("ToPremiseAddress").AsString(200).NotNullable() // Address of the recipient premise
                .WithColumn("IsRecipientExternal").AsBoolean().NotNullable().WithDefaultValue(false) // Indicates if the recipient premise is external
                .WithColumn("Status").AsString(50).NotNullable().WithDefaultValue("Pending") // Transfer status (e.g., Pending, Approved, Rejected)
                .WithColumn("InitiatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime) // Timestamp when the transfer was initiated
                .WithColumn("ApprovedAt").AsDateTime().Nullable() // Timestamp when the transfer was approved
                .WithColumn("RejectedAt").AsDateTime().Nullable() // Timestamp when the transfer was rejected
                .WithColumn("Comments").AsString(500).Nullable(); // Optional comments from the recipient
        }

        public override void Down()
        {
            // Drop the ProductOwnershipTransfers table
            Delete.Table("ProductOwnershipTransfers");
        }
    }
}