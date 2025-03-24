using FluentMigrator;

namespace DataAccess.Migrations
{
    [Migration(2025102502)]
    public class Migration_2025102502_CreateProductsTable : Migration
    {
        public override void Up()
        {
            // Create the Products table
            Create.Table("Products")
                .WithColumn("ProductId").AsInt32().PrimaryKey().Identity() // Auto-incrementing primary key
                .WithColumn("Name").AsString(100).NotNullable() // Name of the product
                .WithColumn("Description").AsString(500).Nullable() // Description of the product
                .WithColumn("Category").AsString(50).NotNullable() // Category of the product (e.g., Meat, Dairy, Feed)
                .WithColumn("PremiseId").AsInt32() // Link to premise
                .WithColumn("ManufacturerId").AsInt32() // Link to manufacturer
                .WithColumn("PermitId").AsInt32() 
                .WithColumn("RegistrationNumber").AsString(100) // Unique registration number
                .WithColumn("ComplianceStatus").AsString(50).NotNullable().WithDefaultValue("Pending") // Compliance status (e.g., Compliant, Non-Compliant)
                .WithColumn("ImageBase64").AsString(int.MaxValue).Nullable() // Base64 encoded image of the product
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime) // Timestamp when the product was created
                .WithColumn("UpdatedAt").AsDateTime().Nullable(); // Optional last update timestamp
        }

        public override void Down()
        {
            // Drop the Products table
            Delete.Table("Products");
        }
    }
}