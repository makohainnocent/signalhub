using FluentMigrator;

namespace DataAccess.Migrations
{
    [Migration(2023102501)] 
    public class Migration_2023102501_CreatePremiseTable : Migration
    {
        public override void Up()
        {
            Create.Table("Premises")
                .WithColumn("PremisesId").AsInt32().PrimaryKey().Identity() // Primary key, auto-incrementing
                .WithColumn("Name").AsString(255).NotNullable() // Name of the premise
                .WithColumn("Coordinates").AsString(100).Nullable() // GPS coordinates
                .WithColumn("Type").AsString(50).Nullable() // Type of premise (e.g., poultry, beef, pigs)
                .WithColumn("OwnerId").AsInt32().NotNullable() // Foreign key to Users table (owner)
                .WithColumn("Status").AsString(50).NotNullable().WithDefaultValue("Pending") // Default status
                .WithColumn("PremiseImage").AsString(int.MaxValue).Nullable() // Base64 encoded image of the premise
                .WithColumn("Province").AsString(100).Nullable() // Province of the premise
                .WithColumn("DistrictConstituency").AsString(100).Nullable() // District and Constituency
                .WithColumn("Ward").AsString(100).Nullable() // Ward of the premise
                .WithColumn("VillageLocalityAddress").AsString(255).Nullable() // Address of the Village or Locality
                .WithColumn("Chiefdom").AsString(100).Nullable() // Chiefdom of the premise
                .WithColumn("Headman").AsString(100).Nullable() // Name of the Headman
                .WithColumn("VeterinaryCamp").AsString(100).Nullable() // Name of the Veterinary Camp
                .WithColumn("CampOfficerNames").AsString(255).Nullable() // Names of the Camp Officers
                .WithColumn("VeterinaryOfficerNames").AsString(255).Nullable() // Names of the Veterinary or Livestock Officers
                .WithColumn("PhysicalPostalAddress").AsString(255).Nullable() // Physical or Postal Address
                .WithColumn("HandlingFacility").AsString(100).Nullable() // Type of handling facility
                .WithColumn("AlternativeAddresses").AsString(255).Nullable() // Alternative addresses for the premise
                .WithColumn("RegisteredAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime) // Registration timestamp
                .WithColumn("UpdatedAt").AsDateTime().Nullable(); // Optional last update timestamp
        }

        public override void Down()
        {
            Delete.Table("Premises"); // Rollback: Drop the Premises table
        }
    }
}