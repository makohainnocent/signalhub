using FluentMigrator;

namespace DataAccess.Migrations
{
    [Migration(2024102405)]
    public class Migration_2024102405_CreateVaccinationTable : Migration
    {
        public override void Up()
        {
            Create.Table("Vaccination")
                .WithColumn("VaccinationId").AsInt32().PrimaryKey().Identity() // Auto-incrementing primary key
                .WithColumn("LivestockId").AsInt32().NotNullable() // Foreign key for the livestock ID
                .WithColumn("UserId").AsInt32().NotNullable() // Foreign key for the user ID
                .WithColumn("FarmId").AsInt32().NotNullable() // Foreign key for the farm ID
                .WithColumn("VaccineName").AsString(255).NotNullable() // Name of the vaccine
                .WithColumn("Manufacturer").AsString(255).NotNullable() // Manufacturer of the vaccine
                .WithColumn("DateAdministered").AsDateTime().NotNullable() // Date the vaccine was administered
                .WithColumn("NextDoseDueDate").AsDateTime().Nullable() // Optional date for the next dose
                .WithColumn("Dosage").AsString(50).NotNullable() // Dosage given, e.g., "5 ml"
                .WithColumn("AdministeredBy").AsString(255).NotNullable() // Name of administrator
                .WithColumn("IsCompleted").AsBoolean().NotNullable().WithDefaultValue(false) // Course completion status
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime) // Creation timestamp
                .WithColumn("UpdatedAt").AsDateTime().Nullable() // Optional last update timestamp
                .WithColumn("Notes").AsString(int.MaxValue).Nullable(); // Additional notes (nvarchar(max))
        }

        public override void Down()
        {
            Delete.Table("Vaccination");
        }
    }
}
