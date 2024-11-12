using Api.Common.Abstractions;
using Asp.Versioning.Builder;
using Asp.Versioning;
using Domain.DocumentManagement.Requests;
using Application.DocumentManagement.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Api.DocumentManagement.DocumentManagementControllers;// Ensure this is the correct namespace for the DocumentManagementControllers
using Domain.Core.Models;
using Api.LivestockManagement.Controllers;
using Application.LivestockManagement.Abstractions;

namespace Api.DocumentManagement.EndpointDefinitions
{
    public class DocumentEndpoints : IEndpointDefinition
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

            var documents = versionedGroup.MapGroup("/documents");

            // POST: Create a document
            documents.MapPost("/", async (IDocumentRepository repo, [FromBody] DocumentCreationRequest request, HttpContext httpContext) =>
            {
                
                return await DocumentManagementControllers.DocumentManagement.CreateDocument(repo, request, httpContext);
            })
            .RequireAuthorization()
            .WithTags("Documents");

            // GET: Retrieve all documents with optional filters
            documents.MapGet("/", async (
                IDocumentRepository repo,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? search = null,
                [FromQuery] int? userId = null,
                [FromQuery] int? farmId = null,
                [FromQuery] int? animalId = null) =>
            {
                return await DocumentManagementControllers.DocumentManagement.GetAllDocumentsAsync(repo, pageNumber, pageSize, search, userId, farmId, animalId);
            })
            .RequireAuthorization()
            .WithTags("Documents");

            // GET: Retrieve a document by ID
            documents.MapGet("/{documentId}", async (IDocumentRepository repo, int documentId) =>
            {
                return await DocumentManagementControllers.DocumentManagement.GetDocumentByIdAsync(repo, documentId);
            })
            .RequireAuthorization()
            .WithTags("Documents");

            // PUT: Update a document by ID
            documents.MapPut("/{documentId}", async (IDocumentRepository repo, int documentId, [FromBody] DocumentUpdateRequest request) =>
            {
                return await DocumentManagementControllers.DocumentManagement.UpdateDocument(repo, documentId, request);
            })
            .RequireAuthorization()
            .WithTags("Documents");

            // DELETE: Delete a document by ID
            documents.MapDelete("/{documentId}", async (IDocumentRepository repo, int documentId) =>
            {
                return await DocumentManagementControllers.DocumentManagement.DeleteDocument(repo, documentId);
            })
            .RequireAuthorization()
            .WithTags("Documents");

            documents.MapGet("/documents/count", async (IDocumentRepository repo, int? userId = null, int? farmId = null) =>
            {
                return await DocumentManagementControllers.DocumentManagement.CountDocuments(repo, userId, farmId);
            })
          // .RequireAuthorization()  // Uncomment if authorization is required
          .WithTags("Documents");
        }
    }
}
