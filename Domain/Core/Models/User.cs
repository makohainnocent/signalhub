using System;
using System.Collections.Generic;

namespace Domain.Core.Models
{
    public class User
    {
        // Primary key
        public int UserId { get; set; }

        // Required fields
        public string Username { get; set; } // Unique username
        public string HashedPassword { get; set; } // Hashed password
        public string Salt { get; set; } // Salt for password hashing
        public string Email { get; set; } // Unique email
        public string FullName { get; set; } // Full name

        // Optional fields
        public string Address { get; set; } // Address (optional)
        public string PhoneNumber { get; set; } // Phone number (optional)

        // Role and security-related fields
        public string Role { get; set; } = "User"; // Default role is "User"
        public int FailedLoginAttempts { get; set; } = 0; // Track failed login attempts
        public bool IsLocked { get; set; } = false; // Track account lockout
        public string PasswordResetToken { get; set; } // Password reset token
        public DateTime? PasswordResetTokenExpiry { get; set; } // Password reset token expiry

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Timestamp when the user was created
        public DateTime? UpdatedAt { get; set; } // Timestamp when the user was last updated
        public DateTime? LastLoginAt { get; set; } // Timestamp of the last login (nullable for new users)

        // Soft delete flag
        public bool IsDeleted { get; set; } = false; // Soft delete flag

        // Track who created the user
        public int? CreatedBy { get; set; } // Nullable for self-registered users

        // Profile and cover photos
        public string CoverPhoto { get; set; } // Cover photo (base64 encoded or URL)
        public string ProfilePhoto { get; set; } // Profile photo (base64 encoded or URL)

        // Navigation properties (if needed)
        public virtual ICollection<User> CreatedUsers { get; set; } // Users created by this user (if applicable)
    }
}