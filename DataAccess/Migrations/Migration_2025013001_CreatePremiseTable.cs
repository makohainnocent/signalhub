using FluentMigrator;

[Migration(2025013001)] // Use the current date in YYYYMMDD format followed by a sequence number
public class Migration_2025013001_CreatePremiseTable : Migration
{
    public override void Up()
    {
        Create.Table("Premises")
            .WithColumn("PremisesId").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("Coordinates").AsString().Nullable()
            .WithColumn("Type").AsString().Nullable()
            .WithColumn("OwnerId").AsInt32().NotNullable()
            .WithColumn("Status").AsString().NotNullable().WithDefaultValue("Pending")
            .WithColumn("PremiseImage").AsString().Nullable()
            .WithColumn("Province").AsString().Nullable()
            .WithColumn("DistrictConstituency").AsString().Nullable()
            .WithColumn("Ward").AsString().Nullable()
            .WithColumn("VillageLocalityAddress").AsString().Nullable()
            .WithColumn("Chiefdom").AsString().Nullable()
            .WithColumn("Headman").AsString().Nullable()
            .WithColumn("VeterinaryCamp").AsString().Nullable()
            .WithColumn("CampOfficerNames").AsString().Nullable()
            .WithColumn("VeterinaryOfficerNames").AsString().Nullable()
            .WithColumn("PhysicalPostalAddress").AsString().Nullable()
            .WithColumn("HandlingFacility").AsString().Nullable()
            .WithColumn("AlternativeAddresses").AsString().Nullable()
            .WithColumn("RegisteredAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn("UpdatedAt").AsDateTime().Nullable();
    }

    public override void Down()
    {
        Delete.Table("Premises");
    }
}