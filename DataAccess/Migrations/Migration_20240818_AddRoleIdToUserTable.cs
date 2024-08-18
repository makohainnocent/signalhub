using FluentMigrator;

[Migration(20240818)]
public class Migration_20240818_AddRoleIdToUserTable : Migration
{
    public override void Up()
    {
        
        Alter.Table("User")
            .AddColumn("RoleId")
            .AsInt32()
            .NotNullable()
            .WithDefaultValue(0);

        
        Create.ForeignKey("FK_User_Roles_RoleId")
            .FromTable("User").ForeignColumn("RoleId")
            .ToTable("Roles").PrimaryColumn("RoleId");
    }

    public override void Down()
    {
       
        Delete.ForeignKey("FK_User_Roles_RoleId").OnTable("User");

       
        Delete.Column("RoleId").FromTable("User");
    }
}
