using FluentMigrator;

namespace DataAccess.Migrations
{
    [Migration(2024102407)]
    public class Migration_2024102407_AddPhotoColumnsToUserTable : Migration
    {
        public override void Up()
        {
            Alter.Table("User")
                .AddColumn("CoverPhoto").AsCustom("nvarchar(max)").Nullable() // Adding CoverPhoto column
                .AddColumn("ProfilePhoto").AsCustom("nvarchar(max)").Nullable(); // Adding ProfilePhoto column
        }

        public override void Down()
        {
            // Directly delete the columns
            Delete.Column("CoverPhoto").FromTable("User"); // Dropping CoverPhoto column
            Delete.Column("ProfilePhoto").FromTable("User"); // Dropping ProfilePhoto column
        }
    }
}
