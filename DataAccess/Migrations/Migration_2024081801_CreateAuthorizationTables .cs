using FluentMigrator;

[Migration(2024081801)]
public class Migration_2024081801_CreateAuthorizationTables : Migration
{
    public override void Up()
    {
        // Create Roles table
        Create.Table("Roles")
            .WithColumn("RoleId").AsInt32().PrimaryKey().Identity()
            .WithColumn("RoleName").AsString(256).NotNullable().Unique();

        // Create Claims table
        Create.Table("Claims")
            .WithColumn("ClaimId").AsInt32().PrimaryKey().Identity()
            .WithColumn("ClaimType").AsString(256).NotNullable()
            .WithColumn("ClaimValue").AsString(256).NotNullable();

        // Create UserRoles table
        Create.Table("UserRoles")
            .WithColumn("UserId").AsInt32().NotNullable()
            .WithColumn("RoleId").AsInt32().NotNullable();

        Create.PrimaryKey("PK_UserRoles")
            .OnTable("UserRoles")
            .Columns("UserId", "RoleId");

        Create.ForeignKey("FK_UserRoles_Users")
            .FromTable("UserRoles").ForeignColumn("UserId")
            .ToTable("Users").PrimaryColumn("UserId"); // Assuming Users table already exists

        Create.ForeignKey("FK_UserRoles_Roles")
            .FromTable("UserRoles").ForeignColumn("RoleId")
            .ToTable("Roles").PrimaryColumn("RoleId");

        // Create RoleClaims table
        Create.Table("RoleClaims")
            .WithColumn("RoleId").AsInt32().NotNullable()
            .WithColumn("ClaimId").AsInt32().NotNullable();

        Create.PrimaryKey("PK_RoleClaims")
            .OnTable("RoleClaims")
            .Columns("RoleId", "ClaimId");

        Create.ForeignKey("FK_RoleClaims_Roles")
            .FromTable("RoleClaims").ForeignColumn("RoleId")
            .ToTable("Roles").PrimaryColumn("RoleId");

        Create.ForeignKey("FK_RoleClaims_Claims")
            .FromTable("RoleClaims").ForeignColumn("ClaimId")
            .ToTable("Claims").PrimaryColumn("ClaimId");

        // Create UserClaims table
        Create.Table("UserClaims")
            .WithColumn("UserId").AsInt32().NotNullable()
            .WithColumn("ClaimId").AsInt32().NotNullable();

        Create.PrimaryKey("PK_UserClaims")
            .OnTable("UserClaims")
            .Columns("UserId", "ClaimId");

        Create.ForeignKey("FK_UserClaims_Users")
            .FromTable("UserClaims").ForeignColumn("UserId")
            .ToTable("Users").PrimaryColumn("UserId"); // Assuming Users table already exists

        Create.ForeignKey("FK_UserClaims_Claims")
            .FromTable("UserClaims").ForeignColumn("ClaimId")
            .ToTable("Claims").PrimaryColumn("ClaimId");
    }

    public override void Down()
    {
        // Drop UserClaims table
        Delete.Table("UserClaims");

        // Drop RoleClaims table
        Delete.Table("RoleClaims");

        // Drop UserRoles table
        Delete.Table("UserRoles");

        // Drop Claims table
        Delete.Table("Claims");

        // Drop Roles table
        Delete.Table("Roles");
    }
}
