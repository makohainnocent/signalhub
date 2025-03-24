using Api.Common.Abstractions;
using Api.Common.Filters;
using Application.TagsManagement.Abstractions;
using Asp.Versioning.Builder;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Domain.Core.Models;
using Api.TagsManagement.Controllers;

namespace Api.TagsManagement.EndPointDefinations
{
    public class TagsManagementEndpoints : IEndpointDefinition
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

            // Tags endpoints
            var tags = versionedGroup.MapGroup("/tags")
                .WithTags("Tags Management");

            tags.MapPost("/", async (ITagsManagementRepository repo, [FromBody] Tag tag, HttpContext httpContext) =>
            {
                return await TagsManagementControllers.CreateTagAsync(repo, tag, httpContext);
            });

            tags.MapGet("/{tagId:int}", async (ITagsManagementRepository repo, int tagId) =>
            {
                return await TagsManagementControllers.GetTagByIdAsync(repo, tagId);
            });

            tags.MapGet("/", async (ITagsManagementRepository repo, int pageNumber = 1, int pageSize = 10, string? search = null) =>
            {
                return await TagsManagementControllers.GetTagsAsync(repo, pageNumber, pageSize, search);
            });

            tags.MapPut("/", async (ITagsManagementRepository repo, [FromBody] Tag tag, HttpContext httpContext) =>
            {
                return await TagsManagementControllers.UpdateTagAsync(repo, tag, httpContext);
            });

            tags.MapDelete("/{tagId:int}", async (ITagsManagementRepository repo, int tagId, HttpContext httpContext) =>
            {
                return await TagsManagementControllers.DeleteTagAsync(repo, tagId, httpContext);
            });

            // Tag Applications endpoints
            var tagApplications = versionedGroup.MapGroup("/tag-applications")
                .WithTags("Tag Applications Management");

            tagApplications.MapPost("/", async (ITagsManagementRepository repo, [FromBody] TagApplication application, HttpContext httpContext) =>
            {
                return await TagsManagementControllers.CreateTagApplicationAsync(repo, application, httpContext);
            });

            tagApplications.MapGet("/{applicationId:int}", async (ITagsManagementRepository repo, int applicationId) =>
            {
                return await TagsManagementControllers.GetTagApplicationByIdAsync(repo, applicationId);
            });

            tagApplications.MapGet("/", async (ITagsManagementRepository repo, int pageNumber = 1, int pageSize = 10,int? applicantId=null, string? search = null,string?agent="no") =>
            {
                return await TagsManagementControllers.GetTagApplicationsAsync(repo, pageNumber, pageSize,applicantId, search,agent);
            });

            tagApplications.MapPut("/", async (ITagsManagementRepository repo, [FromBody] TagApplication application, HttpContext httpContext) =>
            {
                return await TagsManagementControllers.UpdateTagApplicationAsync(repo, application, httpContext);
            });

            tagApplications.MapDelete("/{applicationId:int}", async (ITagsManagementRepository repo, int applicationId, HttpContext httpContext) =>
            {
                return await TagsManagementControllers.DeleteTagApplicationAsync(repo, applicationId, httpContext);
            });

            // Tag Issuances endpoints
            var tagIssuances = versionedGroup.MapGroup("/tag-issuances")
                .WithTags("Tag Issuances Management");

            tagIssuances.MapPost("/", async (ITagsManagementRepository repo, [FromBody] TagIssuance issuance, HttpContext httpContext) =>
            {
                return await TagsManagementControllers.CreateTagIssuanceAsync(repo, issuance, httpContext);
            });
            tagIssuances.MapGet("/{issuanceId:int}", async (ITagsManagementRepository repo, int issuanceId) =>
            {
                return await TagsManagementControllers.GetTagIssuanceByIdAsync(repo, issuanceId);
            });

            tagIssuances.MapGet("/", async (ITagsManagementRepository repo, int pageNumber = 1, int pageSize = 10,int? issuedToId=null, string? search = null,string?agent="no") =>
            {
                return await TagsManagementControllers.GetTagIssuancesAsync(repo, pageNumber, pageSize,issuedToId, search,agent);
            });

            tagIssuances.MapPut("/", async (ITagsManagementRepository repo, [FromBody] TagIssuance issuance, HttpContext httpContext) =>
            {
                return await TagsManagementControllers.UpdateTagIssuanceAsync(repo, issuance, httpContext);
            });

            tagIssuances.MapDelete("/{issuanceId:int}", async (ITagsManagementRepository repo, int issuanceId, HttpContext httpContext) =>
            {
                return await TagsManagementControllers.DeleteTagIssuanceAsync(repo, issuanceId, httpContext);
            });
        }
    }
}