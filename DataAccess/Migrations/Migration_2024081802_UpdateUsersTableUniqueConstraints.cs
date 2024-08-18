using FluentMigrator;

[Migration(2024081801)]
public class Migration_2024081802_UpdateUsersTableUniqueConstraints : Migration
{
    public override void Up()
    {
        // Create unique constraints for Username and Email columns
        Create.UniqueConstraint("UK_Users_Username")
            .OnTable("Users")
            .Column("Username");

        Create.UniqueConstraint("UK_Users_Email")
            .OnTable("Users")
            .Column("Email");
    }

    public override void Down()
    {
        // Drop unique constraints if rolling back
        Delete.UniqueConstraint("UK_Users_Username")
            .FromTable("Users");

        Delete.UniqueConstraint("UK_Users_Email")
            .FromTable("Users");
    }
}
