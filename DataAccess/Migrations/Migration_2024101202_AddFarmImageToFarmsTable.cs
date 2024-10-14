using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Migrations
{
    [Migration(2024101202)]
    public class Migration_2024101202_AddFarmImageToFarmsTable : Migration
    {
        public override void Up()
        {
            Alter.Table("Farm")
                .AddColumn("FarmImage").AsString(int.MaxValue).Nullable(); // Add base64-encoded image column
        }

        public override void Down()
        {
            Delete.Column("FarmImage").FromTable("Farm");
        }
    }
}
