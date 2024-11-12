using FluentMigrator;

namespace DataAccess.Migrations
{
    [Migration(2024102404)]
    public class Migration_2024102404_CreatePostTable : Migration
    {
        public override void Up()
        {
            Create.Table("Post")
                .WithColumn("PostId").AsInt32().PrimaryKey().Identity() // Auto-incrementing primary key
                .WithColumn("UserId").AsInt32().NotNullable() // Foreign key to the user
                .WithColumn("Title").AsString(255).Nullable() // Title of the post
                .WithColumn("PostType").AsString(50).Nullable() // Type of post (e.g., "text", "image", "video")
                .WithColumn("Files").AsCustom("nvarchar(max)").Nullable().WithDefaultValue("[]") // JSON string for files
                .WithColumn("Description").AsString(1000).Nullable() // Optional description
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime) // Creation timestamp
                .WithColumn("UpdatedAt").AsDateTime().Nullable() // Optional last update timestamp
                .WithColumn("IsPublished").AsBoolean().Nullable().WithDefaultValue(true) // Indicates if post is published or draft
                .WithColumn("LikesCount").AsInt32().Nullable().WithDefaultValue(0); // Number of likes or reactions
        }

        public override void Down()
        {
            Delete.Table("Post");
        }
    }
}
