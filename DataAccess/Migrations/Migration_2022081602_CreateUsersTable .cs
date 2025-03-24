using FluentMigrator;

namespace DataAccess.Migrations
{
    [Migration(2022081602)]
    public class Migration_2022081602_CreateUsersTable : Migration
    {
        public override void Up()
        {
            // Create the Users table
            Create.Table("Users")
                .WithColumn("UserId").AsInt32().PrimaryKey().Identity() // Auto-incrementing primary key
                .WithColumn("Username").AsString(50).NotNullable().Unique() // Unique username
                .WithColumn("HashedPassword").AsString(255).NotNullable() // Hashed password
                .WithColumn("Salt").AsString(128).Nullable() // Salt for password hashing
                .WithColumn("Email").AsString(100).NotNullable().Unique() // Unique email
                .WithColumn("FullName").AsString(100).NotNullable() // Full name
                .WithColumn("Address").AsString(200).Nullable() // Address (optional)
                .WithColumn("PhoneNumber").AsString(20).Nullable() // Phone number (optional)
                .WithColumn("Role").AsString(50).NotNullable().WithDefaultValue("User") // User role (e.g., Admin, Inspector, Owner)
                .WithColumn("FailedLoginAttempts").AsInt32().NotNullable().WithDefaultValue(0) // Track failed login attempts
                .WithColumn("IsLocked").AsBoolean().NotNullable().WithDefaultValue(false) // Track account lockout
                .WithColumn("PasswordResetToken").AsString(128).Nullable() // Password reset token
                .WithColumn("PasswordResetTokenExpiry").AsDateTime().Nullable() // Password reset token expiry
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime) // Timestamp when the user was created
                .WithColumn("UpdatedAt").AsDateTime().Nullable() // Timestamp when the user was last updated
                .WithColumn("LastLoginAt").AsDateTime().Nullable() // Timestamp of the last login (nullable for new users)
                .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false) // Soft delete flag
                .WithColumn("CreatedBy").AsInt32().Nullable() // Track who created the user (nullable for self-registered users)
                .WithColumn("CoverPhoto").AsCustom("nvarchar(max)").Nullable() // Cover photo (base64 encoded or URL)
                .WithColumn("ProfilePhoto").AsCustom("nvarchar(max)").Nullable(); // Profile photo (base64 encoded or URL)
        }

        public override void Down()
        {
            // Drop the Users table
            Delete.Table("Users");
        }
    }
}