using FluentMigrator;
using Microsoft.AspNetCore.Http.HttpResults;

[Migration(2024081901)]
public class Migration_2024081901_CreateRefreshTokensTable : Migration
{
    public override void Up()
    {
        Create.Table("RefreshTokens")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("UserId").AsInt32().NotNullable()
            .WithColumn("Token").AsString().NotNullable().Unique()
            .WithColumn("ExpiresAt").AsDateTime().NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("RevokedAt").AsDateTime().Nullable();

        Create.ForeignKey("FK_RefreshTokens_Users")
            .FromTable("RefreshTokens").ForeignColumn("UserId")
            .ToTable("Users").PrimaryColumn("UserId");
    }

    public override void Down()
    {
        Delete.Table("RefreshTokens");
    }
}
