using FluentMigrator;

namespace DataAccess.Migrations
{
    [Migration(2025102503)] // Update migration ID based on your sequence
    public class Migration_2025102503_CreatePremiseOwnersTable : Migration
    {
        public override void Up()
        {
            // Create the PremiseOwners table
            Create.Table("PremiseOwners")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity() // Auto-incrementing primary key
                .WithColumn("CreatedAt").AsDateTime().Nullable()
                .WithColumn("UpdatedAt").AsDateTime().Nullable()
                .WithColumn("Province").AsString(100).Nullable()
                .WithColumn("District").AsString(100).Nullable()
                 .WithColumn("RegisterdById").AsInt32().Nullable()
                .WithColumn("VillageOrAddress").AsString(255).Nullable()
                .WithColumn("Names").AsString(100).Nullable()
                .WithColumn("Surname").AsString(100).Nullable()
                .WithColumn("OtherNames").AsString(100).Nullable()
                .WithColumn("Sex").AsString(10).Nullable()
                .WithColumn("NRC").AsString(50).Nullable()
                .WithColumn("PhoneNumber").AsString(20).Nullable()
                .WithColumn("Email").AsString(255).Nullable()
                .WithColumn("ArtificialPersonName").AsString(255).Nullable()
                .WithColumn("ContactPersonName").AsString(100).Nullable()
                .WithColumn("ContactPersonID").AsString(50).Nullable()
                .WithColumn("ContactPersonPhoneNumber").AsString(20).Nullable()
                .WithColumn("ContactPersonEmail").AsString(255).Nullable();
        }

        public override void Down()
        {
            // Drop the PremiseOwners table
            Delete.Table("PremiseOwners");
        }
    }
}
