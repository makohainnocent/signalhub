using FluentMigrator;


    [Migration(2024081701)] // Use a unique identifier for your migration
    public class Migration_2024081701_CreateRolesAndPermissionsTables : Migration
    {
        public override void Up()
        {
            // Create the Roles table
            Create.Table("Roles")
                .WithColumn("RoleId").AsInt32().PrimaryKey().Identity() // Primary Key with auto-increment
                .WithColumn("RoleName").AsString(50).NotNullable(); // RoleName column

            // Create the Permissions table
            Create.Table("Permissions")
                .WithColumn("PermissionId").AsInt32().PrimaryKey().Identity() // Primary Key with auto-increment
                .WithColumn("ResourceName").AsString(100).NotNullable() // ResourceName column
                .WithColumn("Action").AsString(50).NotNullable(); // Action column

            // Create the RolePermissions table with foreign keys
            Create.Table("RolePermissions")
                .WithColumn("RoleId").AsInt32().NotNullable() // Foreign Key to Roles table
                .WithColumn("PermissionId").AsInt32().NotNullable(); // Foreign Key to Permissions table

            // Add Foreign Key Constraints
            Create.ForeignKey("FK_RolePermissions_Roles")
                .FromTable("RolePermissions").ForeignColumn("RoleId")
                .ToTable("Roles").PrimaryColumn("RoleId");

            Create.ForeignKey("FK_RolePermissions_Permissions")
                .FromTable("RolePermissions").ForeignColumn("PermissionId")
                .ToTable("Permissions").PrimaryColumn("PermissionId");
        }

        public override void Down()
        {
            // Drop Foreign Key Constraints first
            Delete.ForeignKey("FK_RolePermissions_Roles").OnTable("RolePermissions");
            Delete.ForeignKey("FK_RolePermissions_Permissions").OnTable("RolePermissions");

            // Drop the tables
            Delete.Table("RolePermissions");
            Delete.Table("Permissions");
            Delete.Table("Roles");
        }
    }

