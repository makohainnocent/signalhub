using Domain.Common.Responses;
using Domain.RecipientGroups;
using Domain.Recipients;
using Domain.Recipients.Requests;

namespace Application.Recipients.Abstractions
{
    public interface IRecipientsRepository
    {
        // Basic CRUD Operations
        Task<Recipient> CreateRecipientAsync(RecipientCreationRequest request);
        Task<PagedResultResponse<Recipient>> GetRecipientsAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            int? tenantId = null,
            int? userId = null,
            bool? isActive = null);
        Task<Recipient?> GetRecipientByIdAsync(int recipientId);
        Task<Recipient> UpdateRecipientAsync(RecipientUpdateRequest request);
        Task<bool> DeleteRecipientAsync(int recipientId);
        Task<int> CountRecipientsAsync();

        // Specialized Lookups
        Task<Recipient?> GetRecipientByEmailAsync(string email, int tenantId);
        Task<Recipient?> GetRecipientByPhoneAsync(string phoneNumber, int tenantId);
        Task<Recipient?> GetRecipientByExternalIdAsync(string externalId, int tenantId);
        Task<Recipient?> GetRecipientByUserIdAsync(int userId, int tenantId);

        // Status Management
        Task<bool> DeactivateRecipientAsync(int recipientId);
        Task<bool> ActivateRecipientAsync(int recipientId);

        // Preference Management
        Task<bool> UpdatePreferencesAsync(int recipientId, string preferencesJson);

        // Group Membership
        Task<PagedResultResponse<RecipientGroup>> GetRecipientGroupsAsync(
            int recipientId,
            int pageNumber,
            int pageSize);
        Task<int> CountRecipientGroupsAsync(int recipientId);
    }
}