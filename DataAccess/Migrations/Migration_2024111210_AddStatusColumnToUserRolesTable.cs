using System;
using FluentMigrator;

namespace DataAccess.Migrations
{
    [Migration(2024111210)]
    public class Migration_2024111210_AddStatusColumnToUserRolesTable : Migration
    {
        public override void Up()
        {
            // Add Status column to UserRoles table with a default value of "Pending"
            Alter.Table("UserRoles")
                .AddColumn("Status")
                .AsString(50) // Assuming the status is a string with a maximum length of 50
                .NotNullable()
                .WithDefaultValue("Pending");
        }

        public override void Down()
        {
            // Remove Status column from UserRoles table
            Delete.Column("Status").FromTable("UserRoles");
        }
    }
}
