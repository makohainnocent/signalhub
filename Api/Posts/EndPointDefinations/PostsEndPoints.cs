using Api.Common.Abstractions;
using Asp.Versioning.Builder;
using Asp.Versioning;
using Domain.Post.Requests;
using Domain.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Api.Posts.PostControllers;
using Application.Posts.Abstractions;
using System.Security.Claims;

namespace Api.Posts.EndpointDefinitions
{
    public class PostEndpoints : IEndpointDefinition
    {
        public void RegisterEndpoints(WebApplication app)
        {
            ApiVersionSet apiVersionSet = app.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(1))
                .ReportApiVersions()
                .Build();

            RouteGroupBuilder versionedGroup = app
                .MapGroup("/api/v{apiVersion:apiVersion}")
                .WithApiVersionSet(apiVersionSet);

            var posts = versionedGroup.MapGroup("/posts");

            posts.MapPost("/create", async (IPostRepository repo, [FromBody] CreatePostRequest request, HttpContext httpContext) =>
            {
                return await PostController.CreatePostAsync(repo, request, httpContext);
            })
            .RequireAuthorization()
            .WithTags("Posts");

            posts.MapGet("/all", async (
                IPostRepository repo,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? search = null,
                [FromQuery] int? userId = null) =>
            {
                return await PostController.GetAllPostsAsync(repo, pageNumber, pageSize, search, userId);
            })
            .RequireAuthorization()
            .WithTags("Posts");

            posts.MapGet("/{postId}", async (IPostRepository repo, int postId) =>
            {
                return await PostController.GetPostByIdAsync(repo, postId);
            })
            .RequireAuthorization()
            .WithTags("Posts");

            posts.MapPut("/update", async (IPostRepository repo, [FromBody] Post post) =>
            {
                return await PostController.UpdatePostAsync(repo, post);
            })
            .RequireAuthorization()
            .WithTags("Posts");

            posts.MapDelete("/{postId}", async (IPostRepository repo, int postId) =>
            {
                return await PostController.DeletePostAsync(repo, postId);
            })
            .RequireAuthorization()
            .WithTags("Posts");
        }
    }
}
