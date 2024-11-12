using Application.Posts.Abstractions;
using Domain.Post.Requests;
using Domain.Common.Responses;
using Domain.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;

namespace Api.Posts.PostControllers
{
    public class PostController
    {
        public static async Task<IResult> CreatePostAsync(IPostRepository repo, [FromBody] CreatePostRequest request, HttpContext httpContext)
        {
            try
            {
                // Extract the user ID from the claims in the HttpContext
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to create a post by user ID: {UserId}", userId);

                request.UserId = userId;
                Post createdPost = await repo.CreatePostAsync(request);

                Log.Information("Post created successfully with ID: {PostId} by user ID: {UserId}", createdPost.PostId, userId);

                return Results.Created($"/posts/{createdPost.PostId}", createdPost);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating the post.");
                return Results.Problem("An unexpected error occurred while creating the post.");
            }
        }

        public static async Task<IResult> GetAllPostsAsync(
            IPostRepository repo,
            int pageNumber,
            int pageSize,
            string? search = null,
            int? userId = null)
        {
            try
            {
                Log.Information("Retrieving posts with pagination: Page {PageNumber}, PageSize {PageSize}.", pageNumber, pageSize);

                var pagedResult = await repo.GetAllPostsAsync(pageNumber, pageSize, search, userId);
                if (pagedResult == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No posts found" });
                }

                Log.Information("Successfully retrieved {PostCount} posts out of {TotalCount}.", pagedResult.Items.Count(), pagedResult.TotalCount);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving posts.");
                return Results.Problem("An unexpected error occurred while retrieving posts.");
            }
        }

        public static async Task<IResult> GetPostByIdAsync(IPostRepository repo, int postId)
        {
            try
            {
                Log.Information("Retrieving post with ID: {PostId}", postId);

                var post = await repo.GetPostByIdAsync(postId);

                if (post == null)
                {
                    Log.Warning("Post with ID: {PostId} not found.", postId);
                    return Results.NotFound(new { message = "Post not found." });
                }

                Log.Information("Successfully retrieved post with ID: {PostId}", postId);
                return Results.Ok(post);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving post details.");
                return Results.Problem("An unexpected error occurred while retrieving post details.");
            }
        }

        public static async Task<IResult> UpdatePostAsync(IPostRepository repo, Post post)
        {
            try
            {
                Log.Information("Attempting to update post with ID: {PostId}", post.PostId);

                var updatedPost = await repo.UpdatePostAsync(post);

                Log.Information("Post updated successfully with ID: {PostId}", updatedPost.PostId);
                return Results.Ok(updatedPost);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the post.");
                return Results.Problem("An unexpected error occurred while updating the post.");
            }
        }

        public static async Task<IResult> DeletePostAsync(IPostRepository repo, int postId)
        {
            try
            {
                Log.Information("Attempting to delete post with ID: {PostId}", postId);

                var deleted = await repo.DeletePostAsync(postId);

                if (deleted)
                {
                    Log.Information("Post deleted successfully with ID: {PostId}", postId);
                    return Results.Ok(new { message = "Post deleted successfully" });
                }
                else
                {
                    Log.Warning("Post with ID: {PostId} not found.", postId);
                    return Results.NotFound(new { message = $"Post with ID {postId} does not exist." });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting the post.");
                return Results.Problem("An unexpected error occurred while deleting the post.");
            }
        }
    }
}
