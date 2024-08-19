using FluentMigrator;

[Migration(2024081902)]
public class Migration_2024081902_CreateVerificationCodesTable : Migration
{
    public override void Up()
    {
        Create.Table("VerificationCodes")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Email").AsString(255).NotNullable()
            .WithColumn("Code").AsString(6).NotNullable()
            .WithColumn("ExpiryDate").AsDateTime().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("VerificationCodes");
    }
}
