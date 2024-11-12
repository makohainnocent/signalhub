using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Migrations
{
    [Migration(2024102401)]
    public class Migration_2024102401_AddAnimalImageToLivestockTable : Migration
    {
        public override void Up()
        {
            Alter.Table("Livestock")
                .AddColumn("AnimalImage").AsString(int.MaxValue).Nullable(); // Add base64-encoded image column
        }

        public override void Down()
        {
            Delete.Column("AnimalImage").FromTable("Livestock");
        }
    }
}
