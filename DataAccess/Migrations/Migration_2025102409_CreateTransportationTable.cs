using FluentMigrator;

namespace DataAccess.Migrations
{
    [Migration(2025102409)]
    public class Migration_2025102409_CreateTransportationTable : Migration
    {
        public override void Up()
        {
            Create.Table("Transportation")
                .WithColumn("TransportId").AsInt32().PrimaryKey().Identity() // Auto-incrementing primary key
                .WithColumn("PermitId").AsInt32() // Foreign key to Permits table
                .WithColumn("UserId").AsInt32()
                .WithColumn("SourcePremisesId").AsInt32() // Foreign key to Premises table (source)
                .WithColumn("SourceAddress").AsString(200).NotNullable() // String address of the source premises
                .WithColumn("DestinationPremisesId").AsInt32() // Foreign key to Premises table (destination)
                .WithColumn("DestinationAddress").AsString(200).NotNullable() // String address of the destination premises
                .WithColumn("TransporterId").AsInt32()// Foreign key to Users table (transporter)
                .WithColumn("VehicleDetails").AsString(100).NotNullable() // Details of the vehicle
                .WithColumn("StartDate").AsDateTime().NotNullable() // Start date and time of the transportation
                .WithColumn("EndDate").AsDateTime().Nullable() // End date and time of the transportation (optional)
                .WithColumn("ItemsDocument").AsString(int.MaxValue).Nullable() // Base64-encoded document of items being transported
                .WithColumn("ReasonForTransport").AsString(200).NotNullable() // Reason for transportation
                .WithColumn("Description").AsString(500).Nullable() // Additional details about the transportation activity
                .WithColumn("Status").AsString(20).NotNullable().WithDefaultValue("Pending") // Status of the transportation
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime) // Timestamp when the record was created
                .WithColumn("UpdatedAt").AsDateTime().Nullable(); // Optional last update timestamp
        }

        public override void Down()
        {
            Delete.Table("Transportation");
        }
    }
}