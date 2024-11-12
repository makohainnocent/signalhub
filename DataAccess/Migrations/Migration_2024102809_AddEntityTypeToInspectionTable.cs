using System;
using FluentMigrator;

namespace DataAccess.Migrations
{
    [Migration(2024102809)]
    public class Migration_2024102809_AddEntityTypeToInspectionTable : Migration
    {
        public override void Up()
        {
            // Add EntityType column to Inspection table with a default value of 'General'
            Alter.Table("Inspection")
                .AddColumn("EntityType")
                .AsString(50)
                .NotNullable()
                .WithDefaultValue("General");
        }

        public override void Down()
        {
            // Remove EntityType column from Inspection table
            Delete.Column("EntityType").FromTable("Inspection");
        }
    }
}
