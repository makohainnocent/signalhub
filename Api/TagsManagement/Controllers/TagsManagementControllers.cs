using Application.TagsManagement.Abstractions;
using DataAccess.Common.Exceptions;
using Domain.Common.Responses;
using Domain.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api.TagsManagement.Controllers
{
    public static class TagsManagementControllers
    {
        // Tag-related methods

        public static async Task<IResult> CreateTagAsync(ITagsManagementRepository repo, [FromBody] Tag tag, HttpContext httpContext)
        {
            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to create tag for user ID: {UserId}", userId);

                var createdTag = await repo.CreateTagAsync(tag);

                Log.Information("Tag created successfully with ID: {TagId} for user ID: {UserId}", createdTag.TagId, userId);
                return Results.Created($"/tags/{createdTag.TagId}", createdTag);
            }
            catch (ItemAlreadyExistsException ex)
            {
                Log.Warning(ex, "Tag creation failed - tag already exists.");
                return Results.Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating the tag.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetTagByIdAsync(ITagsManagementRepository repo, int tagId)
        {
            try
            {
                Log.Information("Attempting to retrieve tag with ID: {TagId}", tagId);

                var tag = await repo.GetTagByIdAsync(tagId);

                if (tag == null)
                {
                    Log.Warning("Tag with ID: {TagId} not found.", tagId);
                    return Results.NotFound(new { message = "Tag not found." });
                }

                Log.Information("Successfully retrieved tag with ID: {TagId}", tagId);
                return Results.Ok(tag);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the tag.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetTagsAsync(ITagsManagementRepository repo, int pageNumber = 1, int pageSize = 10, string? search = null)
        {
            try
            {
                Log.Information("Attempting to retrieve tags with pagination: Page {PageNumber}, PageSize {PageSize}.", pageNumber, pageSize);

                var pagedResult = await repo.GetTagsAsync(pageNumber, pageSize, search);

                if (pagedResult == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No tags found." });
                }

                Log.Information("Successfully retrieved {TagCount} tags out of {TotalCount}.", pagedResult.Items.Count(), pagedResult.TotalCount);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving tags.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> UpdateTagAsync(ITagsManagementRepository repo, [FromBody] Tag tag, HttpContext httpContext)
        {
            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to update tag with ID: {TagId} for user ID: {UserId}", tag.TagId, userId);

                var updated = await repo.UpdateTagAsync(tag);

                if (updated)
                {
                    Log.Information("Tag updated successfully with ID: {TagId} for user ID: {UserId}", tag.TagId, userId);
                    return Results.Ok(new { message = "Tag updated successfully." });
                }
                else
                {
                    Log.Warning("Tag update failed - tag does not exist.");
                    return Results.NotFound(new { message = "Tag not found." });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the tag.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> DeleteTagAsync(ITagsManagementRepository repo, int tagId, HttpContext httpContext)
        {
            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to delete tag with ID: {TagId} for user ID: {UserId}", tagId, userId);

                var deleted = await repo.DeleteTagAsync(tagId);

                if (deleted)
                {
                    Log.Information("Tag deleted successfully with ID: {TagId} for user ID: {UserId}", tagId, userId);
                    return Results.Ok(new { message = "Tag deleted successfully." });
                }
                else
                {
                    Log.Warning("Tag deletion failed - tag does not exist.");
                    return Results.NotFound(new { message = "Tag not found." });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting the tag.");
                return Results.Problem(ex.Message);
            }
        }

        // TagApplication-related methods

        public static async Task<IResult> CreateTagApplicationAsync(ITagsManagementRepository repo, [FromBody] TagApplication application, HttpContext httpContext)
        {
            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to create tag application for user ID: {UserId}", userId);

                application.AppliedBy = userId;
                var createdApplication = await repo.CreateTagApplicationAsync(application);

                Log.Information("Tag application created successfully with ID: {ApplicationId} for user ID: {UserId}", createdApplication.ApplicationId, userId);
                return Results.Created($"/tag-applications/{createdApplication.ApplicationId}", createdApplication);
            }
            catch (ItemAlreadyExistsException ex)
            {
                Log.Warning(ex, "Tag application creation failed - application already exists.");
                return Results.Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating the tag application.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetTagApplicationByIdAsync(ITagsManagementRepository repo, int applicationId)
        {
            try
            {
                Log.Information("Attempting to retrieve tag application with ID: {ApplicationId}", applicationId);

                var application = await repo.GetTagApplicationByIdAsync(applicationId);

                if (application == null)
                {
                    Log.Warning("Tag application with ID: {ApplicationId} not found.", applicationId);
                    return Results.NotFound(new { message = "Tag application not found." });
                }

                Log.Information("Successfully retrieved tag application with ID: {ApplicationId}", applicationId);
                return Results.Ok(application);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the tag application.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetTagApplicationsAsync(ITagsManagementRepository repo, int pageNumber = 1, int pageSize = 10,int? applicantId=null, string? search = null,string?agent="no")
        {
            try
            {
                Log.Information("Attempting to retrieve tag applications with pagination: Page {PageNumber}, PageSize {PageSize}.", pageNumber, pageSize);

                var pagedResult = await repo.GetTagApplicationsAsync(pageNumber, pageSize,applicantId, search,agent);

                if (pagedResult == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No tag applications found." });
                }

                Log.Information("Successfully retrieved {ApplicationCount} tag applications out of {TotalCount}.", pagedResult.Items.Count(), pagedResult.TotalCount);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving tag applications.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> UpdateTagApplicationAsync(ITagsManagementRepository repo, [FromBody] TagApplication application, HttpContext httpContext)
        {
            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to update tag application with ID: {ApplicationId} for user ID: {UserId}", application.ApplicationId, userId);

                var updated = await repo.UpdateTagApplicationAsync(application);

                if (updated)
                {
                    Log.Information("Tag application updated successfully with ID: {ApplicationId} for user ID: {UserId}", application.ApplicationId, userId);
                    return Results.Ok(new { message = "Tag application updated successfully." });
                }
                else
                {
                    Log.Warning("Tag application update failed - application does not exist.");
                    return Results.NotFound(new { message = "Tag application not found." });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the tag application.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> DeleteTagApplicationAsync(ITagsManagementRepository repo, int applicationId, HttpContext httpContext)
        {
            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to delete tag application with ID: {ApplicationId} for user ID: {UserId}", applicationId, userId);

                var deleted = await repo.DeleteTagApplicationAsync(applicationId);

                if (deleted)
                {
                    Log.Information("Tag application deleted successfully with ID: {ApplicationId} for user ID: {UserId}", applicationId, userId);
                    return Results.Ok(new { message = "Tag application deleted successfully." });
                }
                else
                {
                    Log.Warning("Tag application deletion failed - application does not exist.");
                    return Results.NotFound(new { message = "Tag application not found." });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting the tag application.");
                return Results.Problem(ex.Message);
            }
        }

        // TagIssuance-related methods

        public static async Task<IResult> CreateTagIssuanceAsync(ITagsManagementRepository repo, [FromBody] TagIssuance issuance, HttpContext httpContext)
        {
            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to create tag issuance for user ID: {UserId}", userId);

                issuance.IssuedBy = userId;
                var createdIssuance = await repo.CreateTagIssuanceAsync(issuance);

                Log.Information("Tag issuance created successfully with ID: {IssuanceId} for user ID: {UserId}", createdIssuance.IssuanceId, userId);
                return Results.Created($"/tag-issuances/{createdIssuance.IssuanceId}", createdIssuance);
            }
            catch (ItemAlreadyExistsException ex)
            {
                Log.Warning(ex, "Tag issuance creation failed - issuance already exists.");
                return Results.Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating the tag issuance.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetTagIssuanceByIdAsync(ITagsManagementRepository repo, int issuanceId)
        {
            try
            {
                Log.Information("Attempting to retrieve tag issuance with ID: {IssuanceId}", issuanceId);

                var issuance = await repo.GetTagIssuanceByIdAsync(issuanceId);

                if (issuance == null)
                {
                    Log.Warning("Tag issuance with ID: {IssuanceId} not found.", issuanceId);
                    return Results.NotFound(new { message = "Tag issuance not found." });
                }

                Log.Information("Successfully retrieved tag issuance with ID: {IssuanceId}", issuanceId);
                return Results.Ok(issuance);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the tag issuance.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetTagIssuancesAsync(ITagsManagementRepository repo, int pageNumber = 1, int pageSize = 10,int? issuedToId=null, string? search = null,string?agent="no")
        {
            try
            {
                Log.Information("Attempting to retrieve tag issuances with pagination: Page {PageNumber}, PageSize {PageSize}.", pageNumber, pageSize);

                var pagedResult = await repo.GetTagIssuancesAsync(pageNumber, pageSize,issuedToId, search,agent);

                if (pagedResult == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No tag issuances found." });
                }

                Log.Information("Successfully retrieved {IssuanceCount} tag issuances out of {TotalCount}.", pagedResult.Items.Count(), pagedResult.TotalCount);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving tag issuances.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> UpdateTagIssuanceAsync(ITagsManagementRepository repo, [FromBody] TagIssuance issuance, HttpContext httpContext)
        {
            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to update tag issuance with ID: {IssuanceId} for user ID: {UserId}", issuance.IssuanceId, userId);

                var updated = await repo.UpdateTagIssuanceAsync(issuance);

                if (updated)
                {
                    Log.Information("Tag issuance updated successfully with ID: {IssuanceId} for user ID: {UserId}", issuance.IssuanceId, userId);
                    return Results.Ok(new { message = "Tag issuance updated successfully." });
                }
                else
                {
                    Log.Warning("Tag issuance update failed - issuance does not exist.");
                    return Results.NotFound(new { message = "Tag issuance not found." });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the tag issuance.");
                return Results.Problem(ex.Message);
            }
        }

        public static async Task<IResult> DeleteTagIssuanceAsync(ITagsManagementRepository repo, int issuanceId, HttpContext httpContext)
        {
            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to delete tag issuance with ID: {IssuanceId} for user ID: {UserId}", issuanceId, userId);

                var deleted = await repo.DeleteTagIssuanceAsync(issuanceId);

                if (deleted)
                {
                    Log.Information("Tag issuance deleted successfully with ID: {IssuanceId} for user ID: {UserId}", issuanceId, userId);
                    return Results.Ok(new { message = "Tag issuance deleted successfully." });
                }
                else
                {
                    Log.Warning("Tag issuance deletion failed - issuance does not exist.");
                    return Results.NotFound(new { message = "Tag issuance not found." });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting the tag issuance.");
                return Results.Problem(ex.Message);
            }
        }
    }
}