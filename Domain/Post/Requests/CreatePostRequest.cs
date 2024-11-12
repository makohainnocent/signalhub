using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Post.Requests
{
    public class CreatePostRequest
    {
        public int UserId { get; set; }                       
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string PostType { get; set; } = string.Empty; 
        public string Files { get; set; } = "[]";
    }
}
