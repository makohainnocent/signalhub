using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Core.Models
{
    public class Post
    {
        public int PostId { get; set; }                        // Unique identifier for the post
        public int UserId { get; set; }                        // ID of the user who posted
        public string Title { get; set; } = string.Empty;      // Title of the post
        public string PostType { get; set; } = string.Empty;   // Type of post (e.g., "text", "image", "video")

        // JSON string to hold a list of files encoded as base64 strings or URLs
        public string Files { get; set; } = "[]";              // JSON string (e.g., ["file1_base64", "file2_base64"])

        public string? Description { get; set; }               // Optional description of the post
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // Creation timestamp
        public DateTime? UpdatedAt { get; set; }               // Optional last update timestamp

        public bool IsPublished { get; set; } = true;          // Indicates if the post is published or draft
        public int LikesCount { get; set; } = 0;

        // Add this property to hold user details
        public User? User { get; set; } = new User();
    }
}
