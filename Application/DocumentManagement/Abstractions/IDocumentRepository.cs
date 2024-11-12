using Domain.Common.Responses;
using Domain.Core.Models;
using Domain.DocumentManagement.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DocumentManagement.Abstractions
{
    public interface IDocumentRepository
    {
        // Method to create a new document
        Task<Document> CreateDocumentAsync(DocumentCreationRequest request);

        // Method to retrieve a document by its ID
        Task<Document?> GetDocumentByIdAsync(int documentId);

        // Method to retrieve all documents with pagination and optional filtering
        Task<PagedResultResponse<Document>> GetAllDocumentsAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            int? userId = null,
            int? farmId = null,
            int? animalId = null);

        // Method to update an existing document
        Task<Document> UpdateDocumentAsync(int documentId, DocumentUpdateRequest request);

        // Method to delete a document by its ID
        Task<bool> DeleteDocumentAsync(int documentId);

        Task<int> CountDocumentsAsync(int? userId = null, int? farmId = null);
    }
}
