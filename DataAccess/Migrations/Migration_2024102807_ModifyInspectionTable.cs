using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Migrations
{
    using FluentMigrator;

    namespace DataAccess.Migrations
    {
        [Migration(2024102807)]
        public class Migration_2024102807_ModifyInspectionTable : Migration
        {
            public override void Up()
            {
                // Rename column LivestockId to EntityIds
                Rename.Column("LivestockId").OnTable("Inspection").To("EntityIds");

                // Alter EntityIds column to nvarchar(max) for storing multiple IDs
                Alter.Column("EntityIds").OnTable("Inspection")
                    .AsCustom("nvarchar(max)")
                    .NotNullable();

                // Add CreatedAt and UpdatedAt default values for UTC timestamps
                Alter.Column("CreatedAt").OnTable("Inspection")
                    .AsDateTime()
                    .WithDefault(SystemMethods.CurrentUTCDateTime)
                    .NotNullable();

                Alter.Column("UpdatedAt").OnTable("Inspection")
                    .AsDateTime()
                    .WithDefault(SystemMethods.CurrentUTCDateTime)
                    .NotNullable();
            }

            public override void Down()
            {
                // Revert EntityIds column back to an integer and rename to LivestockId
                Rename.Column("EntityIds").OnTable("Inspection").To("LivestockId");
                Alter.Column("LivestockId").OnTable("Inspection")
                    .AsInt32()
                    .NotNullable();

                // Remove default values on CreatedAt and UpdatedAt columns
                Alter.Column("CreatedAt").OnTable("Inspection")
                    .AsDateTime()
                    .Nullable();

                Alter.Column("UpdatedAt").OnTable("Inspection")
                    .AsDateTime()
                    .Nullable();
            }
        }
    }

}
