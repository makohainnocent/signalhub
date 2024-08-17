using FluentMigrator;

[Migration(2024081602)]
public class Migration_2024081601_CreateAnimalTrackingTables : Migration
{
    public override void Up()
    {
        // User Table
        Create.Table("User")
            .WithColumn("UserId").AsInt32().PrimaryKey().Identity()
            .WithColumn("Username").AsString().NotNullable()
            .WithColumn("HashedPassword").AsString().NotNullable()
            .WithColumn("Email").AsString().NotNullable()
            .WithColumn("FullName").AsString().NotNullable()
            .WithColumn("Address").AsString().Nullable()
            .WithColumn("CreatedAt").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime).NotNullable()
            .WithColumn("LastLoginAt").AsDateTime().NotNullable();

        // Farm Table
        Create.Table("Farm")
            .WithColumn("FarmId").AsInt32().PrimaryKey().Identity()
            .WithColumn("UserId").AsInt32().NotNullable()
            .WithColumn("FarmName").AsString().NotNullable()
            .WithColumn("Location").AsString().NotNullable()
            .WithColumn("Area").AsDecimal().NotNullable() // in acres
            .WithColumn("CreatedAt").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime).NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime).NotNullable();

        // FarmGeofencing Table
        Create.Table("FarmGeofencing")
            .WithColumn("GeofenceId").AsInt32().PrimaryKey().Identity()
            .WithColumn("FarmId").AsInt32().NotNullable()
            .WithColumn("Latitude").AsDecimal().NotNullable()
            .WithColumn("Longitude").AsDecimal().NotNullable()
            .WithColumn("Radius").AsDouble().NotNullable() // in meters
            .WithColumn("CreatedAt").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime).NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime).NotNullable();

        // Livestock Table
        Create.Table("Livestock")
            .WithColumn("LivestockId").AsInt32().PrimaryKey().Identity()
            .WithColumn("FarmId").AsInt32().NotNullable()
            .WithColumn("UserId").AsInt32().NotNullable()
            .WithColumn("Species").AsString().NotNullable()
            .WithColumn("Breed").AsString().NotNullable()
            .WithColumn("DateOfBirth").AsDateTime().NotNullable()
            .WithColumn("HealthStatus").AsString().Nullable()
            .WithColumn("IdentificationMark").AsString().Nullable()
            .WithColumn("CreatedAt").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime).NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime).NotNullable();

        // HealthRecord Table
        Create.Table("HealthRecord")
            .WithColumn("RecordId").AsInt32().PrimaryKey().Identity()
            .WithColumn("LivestockId").AsInt32().NotNullable()
            .WithColumn("UserId").AsInt32().NotNullable()
            .WithColumn("DateOfVisit").AsDateTime().NotNullable()
            .WithColumn("Diagnosis").AsString().Nullable()
            .WithColumn("Treatment").AsString().Nullable()
            .WithColumn("FollowUpDate").AsDateTime().Nullable()
            .WithColumn("CreatedAt").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime).NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime).NotNullable();

        // Feed Table
        Create.Table("Feed")
            .WithColumn("FeedId").AsInt32().PrimaryKey().Identity()
            .WithColumn("FeedName").AsString().NotNullable()
            .WithColumn("FeedType").AsString().NotNullable()
            .WithColumn("ManufactureDate").AsDateTime().NotNullable()
            .WithColumn("ExpiryDate").AsDateTime().NotNullable()
            .WithColumn("Supplier").AsString().Nullable()
            .WithColumn("Details").AsString().Nullable()
            .WithColumn("CreatedAt").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime).NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime).NotNullable();

        // FeedTransaction Table
        Create.Table("FeedTransaction")
            .WithColumn("TransactionId").AsInt32().PrimaryKey().Identity()
            .WithColumn("FeedId").AsInt32().NotNullable()
            .WithColumn("TransactionDate").AsDateTime().NotNullable()
            .WithColumn("Quantity").AsDecimal().NotNullable()
            .WithColumn("TransactionType").AsString().NotNullable() // e.g., Purchase, Sale
            .WithColumn("Details").AsString().Nullable()
            .WithColumn("CreatedAt").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime).NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime).NotNullable();

        // Directive Table
        Create.Table("Directive")
            .WithColumn("DirectiveId").AsInt32().PrimaryKey().Identity()
            .WithColumn("LivestockId").AsInt32().NotNullable()
            .WithColumn("DirectiveDate").AsDateTime().NotNullable()
            .WithColumn("DirectiveDetails").AsString().Nullable()
            .WithColumn("CreatedAt").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime).NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime).NotNullable();

        // Notification Table
        Create.Table("Notification")
            .WithColumn("NotificationId").AsInt32().PrimaryKey().Identity()
            .WithColumn("UserId").AsInt32().NotNullable()
            .WithColumn("Message").AsString().NotNullable()
            .WithColumn("Date").AsDateTime().NotNullable()
            .WithColumn("Status").AsString().NotNullable()
            .WithColumn("CreatedAt").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime).NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime).NotNullable();

        // Tag Table
        Create.Table("Tag")
            .WithColumn("TagId").AsInt32().PrimaryKey().Identity()
            .WithColumn("TagCode").AsString().NotNullable()
            .WithColumn("IssuedDate").AsDateTime().NotNullable()
            .WithColumn("IssuedBy").AsInt32().NotNullable() // UserId of the issuer
            .WithColumn("LivestockId").AsInt32().NotNullable()
            .WithColumn("Status").AsString().NotNullable() // e.g., Active, Inactive, Revoked
            .WithColumn("Comments").AsString().Nullable()
            .WithColumn("CreatedAt").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime).NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime).NotNullable();

        // Permit Table
        Create.Table("Permit")
            .WithColumn("PermitId").AsInt32().PrimaryKey().Identity()
            .WithColumn("LivestockId").AsInt32().NotNullable()
            .WithColumn("IssuedDate").AsDateTime().NotNullable()
            .WithColumn("ExpiryDate").AsDateTime().NotNullable()
            .WithColumn("PermitType").AsString().NotNullable() // e.g., Transport, Export, Import
            .WithColumn("Status").AsString().NotNullable()
            .WithColumn("Details").AsString().Nullable()
            .WithColumn("CreatedAt").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime).NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime).NotNullable();

        // Inspection Table
        Create.Table("Inspection")
            .WithColumn("InspectionId").AsInt32().PrimaryKey().Identity()
            .WithColumn("LivestockId").AsInt32().NotNullable()
            .WithColumn("UserId").AsInt32().NotNullable()
            .WithColumn("InspectionDate").AsDateTime().NotNullable()
            .WithColumn("Outcome").AsString().Nullable()
            .WithColumn("Notes").AsString().Nullable()
            .WithColumn("FollowUpDate").AsDateTime().Nullable()
            .WithColumn("CreatedAt").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime).NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime).NotNullable();

        // Transporter Table
        Create.Table("Transporter")
            .WithColumn("TransporterId").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("ContactDetails").AsString().NotNullable()
            .WithColumn("VehicleDetails").AsString().Nullable()
            .WithColumn("ComplianceStatus").AsString().Nullable();

        // VeterinaryOfficer Table
        Create.Table("VeterinaryOfficer")
            .WithColumn("OfficerId").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("Qualification").AsString().NotNullable()
            .WithColumn("LicenseNumber").AsString().Nullable()
            .WithColumn("ContactDetails").AsString().Nullable();

        // AgroVeterinaryShop Table
        Create.Table("AgroVeterinaryShop")
            .WithColumn("ShopId").AsInt32().PrimaryKey().Identity()
            .WithColumn("ShopName").AsString().NotNullable()
            .WithColumn("Location").AsString().NotNullable()
            .WithColumn("ContactDetails").AsString().Nullable()
            .WithColumn("AuthorizedProducts").AsString().Nullable();

        // FeedBusinessOperator Table
        Create.Table("FeedBusinessOperator")
            .WithColumn("OperatorId").AsInt32().PrimaryKey().Identity()
            .WithColumn("OperatorName").AsString().NotNullable()
            .WithColumn("ContactDetails").AsString().NotNullable()
            .WithColumn("ComplianceStatus").AsString().Nullable();

        // AquacultureLicense Table
        Create.Table("AquacultureLicense")
            .WithColumn("LicenseId").AsInt32().PrimaryKey().Identity()
            .WithColumn("LicenseNumber").AsString().NotNullable()
            .WithColumn("IssuedDate").AsDateTime().NotNullable()
            .WithColumn("ExpiryDate").AsDateTime().NotNullable()
            .WithColumn("FacilityName").AsString().NotNullable()
            .WithColumn("FacilityLocation").AsString().NotNullable()
            .WithColumn("EnvironmentalImpactAssessment").AsString().Nullable()
            .WithColumn("WaterUsePermit").AsString().Nullable()
            .WithColumn("ChemicalRestrictions").AsString().Nullable();

        // ComplianceMonitor Table
        Create.Table("ComplianceMonitor")
            .WithColumn("MonitorId").AsInt32().PrimaryKey().Identity()
            .WithColumn("MonitoringDate").AsDateTime().NotNullable()
            .WithColumn("ComplianceStatus").AsString().NotNullable()
            .WithColumn("Notes").AsString().Nullable()
            .WithColumn("Penalties").AsString().Nullable();

        
    }

    public override void Down()
    {
        

        // Drop Tables
        Delete.Table("ComplianceMonitor");
        Delete.Table("AquacultureLicense");
        Delete.Table("FeedBusinessOperator");
        Delete.Table("AgroVeterinaryShop");
        Delete.Table("VeterinaryOfficer");
        Delete.Table("Transporter");
        Delete.Table("Inspection");
        Delete.Table("Permit");
        Delete.Table("Tag");
        Delete.Table("Notification");
        Delete.Table("Directive");
        Delete.Table("FeedTransaction");
        Delete.Table("Feed");
        Delete.Table("HealthRecord");
        Delete.Table("Livestock");
        Delete.Table("FarmGeofencing");
        Delete.Table("Farm");
        Delete.Table("User");
    }
}
