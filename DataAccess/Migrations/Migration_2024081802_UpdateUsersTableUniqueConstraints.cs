using FluentMigrator;

[Migration(2024081802)]
public class Migration_2024081802_UpdateUsersTableUniqueConstraints : Migration
{
    public override void Up()
    {
        // Create unique constraints for Username and Email columns
        Create.UniqueConstraint("UK_Users_Username")
            .OnTable("User")
            .Column("Username");

        Create.UniqueConstraint("UK_Users_Email")
            .OnTable("User")
            .Column("Email");
    }

    public override void Down()
    {
        // Drop unique constraints if rolling back
        Delete.UniqueConstraint("UK_Users_Username")
            .FromTable("User");

        Delete.UniqueConstraint("UK_Users_Email")
            .FromTable("User");
    }
}
