using Application.DocumentManagement.Abstractions;
using Application.LivestockManagement.Abstractions;
using Domain.Common.Responses;
using Domain.Core.Models;
using Domain.DocumentManagement.Requests;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;

namespace Api.DocumentManagement.DocumentManagementControllers
{
   

        public class DocumentManagement
        {
            public static async Task<IResult> CreateDocument(IDocumentRepository repo, [FromBody] DocumentCreationRequest request, HttpContext httpContext)
            {
                try
                {
                    var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                    {
                        return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                    }

                    Log.Information("Attempting to create document with description: {Title} by user ID: {UserId}", request.Description, userId);

                    Document createdDocument = await repo.CreateDocumentAsync(request);

                    Log.Information("Document created successfully with ID: {DocumentId} by user ID: {UserId}", createdDocument.DocumentId, userId);
                    return Results.Created($"/documents/{createdDocument.DocumentId}", createdDocument);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while creating the document.");
                    return Results.Problem(ex.Message);
                }
            }

            public static async Task<IResult> GetAllDocumentsAsync(IDocumentRepository repo, int pageNumber, int pageSize, string? search = null, int? userId = null, int? farmId = null, int? animalId = null)
            {
                try
                {
                    Log.Information("Attempting to retrieve documents with pagination: Page {PageNumber}, PageSize {PageSize}.", pageNumber, pageSize);

                    PagedResultResponse<Document> pagedResult = await repo.GetAllDocumentsAsync(pageNumber, pageSize, search, userId, farmId, animalId);

                    if (pagedResult.Items == null || !pagedResult.Items.Any())
                    {
                        return Results.NotFound(new { message = "No documents found." });
                    }

                    Log.Information("Successfully retrieved {DocumentCount} documents out of {TotalCount}.", pagedResult.Items.Count(), pagedResult.TotalCount);
                    return Results.Ok(pagedResult);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while retrieving documents.");
                    return Results.Problem(ex.Message);
                }
            }

            public static async Task<IResult> GetDocumentByIdAsync(IDocumentRepository repo, int documentId)
            {
                try
                {
                    Log.Information("Attempting to retrieve document with ID: {DocumentId}", documentId);

                    Document? document = await repo.GetDocumentByIdAsync(documentId);

                    if (document == null)
                    {
                        Log.Warning("Document with ID: {DocumentId} not found.", documentId);
                        return Results.NotFound(new { message = "Document not found." });
                    }

                    Log.Information("Successfully retrieved document with ID: {DocumentId}", documentId);
                    return Results.Ok(document);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while retrieving document details.");
                    return Results.Problem(ex.Message);
                }
            }

            public static async Task<IResult> UpdateDocument(IDocumentRepository repo, int documentId, [FromBody] DocumentUpdateRequest request)
            {
                try
                {
                    Log.Information("Attempting to update document with ID: {DocumentId}", documentId);

                    Document updatedDocument = await repo.UpdateDocumentAsync(documentId, request);

                    Log.Information("Document updated successfully with ID: {DocumentId}", updatedDocument.DocumentId);
                    return Results.Ok(updatedDocument);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while updating the document.");
                    return Results.Problem(ex.Message);
                }
            }

            public static async Task<IResult> DeleteDocument(IDocumentRepository repo, int documentId)
            {
                try
                {
                    Log.Information("Attempting to delete document with ID: {DocumentId}", documentId);

                    bool deleted = await repo.DeleteDocumentAsync(documentId);

                    if (deleted)
                    {
                        Log.Information("Document deleted successfully with ID: {DocumentId}", documentId);
                        return Results.Ok("Document deleted successfully.");
                    }
                    else
                    {
                        return Results.NotFound(new { message = $"Document with ID {documentId} does not exist." });
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while deleting the document.");
                    return Results.Problem(ex.Message);
                }
            }

        public static async Task<IResult> CountDocuments(IDocumentRepository repo,int? userId = null,int? farmId = null)
        {
            try
            {
                
                int documentsCount = await repo.CountDocumentsAsync(userId, farmId);

               
                if (documentsCount == 0)
                {
                    return Results.NotFound(new { message = "No documents records found for the specified criteria." });
                }

                return Results.Ok(new { count = documentsCount });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while counting documents records.");
                return Results.Problem(ex.Message);
            }
        }
    }
    
}
