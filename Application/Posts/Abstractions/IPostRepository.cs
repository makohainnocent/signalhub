using Domain.Common.Responses;
using Domain.Core.Models;
using Domain.Post.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Posts.Abstractions
{
    public interface IPostRepository
    {
        Task<Post> CreatePostAsync(CreatePostRequest request);
        Task<Post?> GetPostByIdAsync(int postId);
        Task<Post> UpdatePostAsync(Post post);
        Task<bool> DeletePostAsync(int postId);
        Task<PagedResultResponse<Post>> GetAllPostsAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            int? userId = null);
    }
}
