using FluentMigrator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DataAccess.Migrations
{
    [Migration(2025102413)]
    public class Migration_2025102413_CreateAnimalsTable : Migration
    {
        public override void Up()
        {
            Create.Table("Animals")
                .WithColumn("AnimalId").AsInt32().PrimaryKey().Identity() // Auto-incrementing primary key
                .WithColumn("Species").AsString(50).NotNullable() // Species of the animal
                .WithColumn("Breed").AsString(50).Nullable() // Breed of the animal (optional)
                .WithColumn("BirthDate").AsString(50).NotNullable() // Birth date of the animal
                .WithColumn("Color").AsString(50).NotNullable() // Color of the animal
                .WithColumn("Description").AsString(255).NotNullable() // Description of the animal
                .WithColumn("Name").AsString(100).NotNullable() // Name of the animal
                .WithColumn("HealthStatus").AsString(50).NotNullable() // Health status of the animal
                .WithColumn("IdentificationMark").AsString(100).NotNullable().Unique() // Unique identification mark
                .WithColumn("OwnerId").AsInt32().NotNullable() // Foreign key to Users table (owner)
                .WithColumn("PremisesId").AsInt32().NotNullable() // Foreign key to Premises table
                .WithColumn("Status").AsString(50).WithDefaultValue("Alive") // Status of the animal
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime) // Timestamp when the animal was registered
                .WithColumn("UpdatedAt").AsDateTime().Nullable() // Optional last update timestamp
                .WithColumn("AnimalImage").AsString(int.MaxValue).Nullable(); // Base64 encoded image of the animal (optional)
        }

        public override void Down()
        {
            Delete.Table("Animals");
        }
    }
}
