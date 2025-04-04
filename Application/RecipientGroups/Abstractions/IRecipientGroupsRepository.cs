using Domain.Common.Responses;
using Domain.RecipientGroups;
using Domain.RecipientGroups.Requests;
using Domain.Recipients;

namespace Application.RecipientGroups.Abstractions
{
    public interface IRecipientGroupsRepository
    {
        Task<RecipientGroup> CreateRecipientGroupAsync(RecipientGroupCreationRequest request);
        Task<PagedResultResponse<RecipientGroup>> GetRecipientGroupsAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            int? tenantId = null,
            int? createdByUserId = null);
        Task<RecipientGroup?> GetRecipientGroupByIdAsync(int groupId);
        Task<RecipientGroup> UpdateRecipientGroupAsync(RecipientGroupUpdateRequest request);
        Task<bool> DeleteRecipientGroupAsync(int groupId);
        Task<int> CountRecipientGroupsAsync();

        // Recipient Group specific methods
        Task<bool> AddRecipientToGroupAsync(int groupId, int recipientId, int addedByUserId);
        Task<bool> RemoveRecipientFromGroupAsync(int groupId, int recipientId);
        Task<PagedResultResponse<Recipient>> GetGroupRecipientsAsync(
            int groupId,
            int pageNumber,
            int pageSize,
            string? search = null);
        Task<int> CountGroupRecipientsAsync(int groupId);
    }
}