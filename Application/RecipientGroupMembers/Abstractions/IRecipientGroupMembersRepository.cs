using Domain.Common.Responses;
using Domain.RecipientGroups;
using Domain.Recipients;

namespace Application.RecipientGroups.Abstractions
{
    public interface IRecipientGroupMembersRepository
    {
        // Membership Operations
        Task<bool> AddRecipientToGroupAsync(int groupId, int recipientId, int addedByUserId);
        Task<bool> RemoveRecipientFromGroupAsync(int groupId, int recipientId);
        Task<bool> IsRecipientInGroupAsync(int groupId, int recipientId);

        // Bulk Operations
        Task<int> AddMultipleRecipientsToGroupAsync(int groupId, IEnumerable<int> recipientIds, int addedByUserId);
        Task<int> RemoveMultipleRecipientsFromGroupAsync(int groupId, IEnumerable<int> recipientIds);

        // Query Operations
        Task<PagedResultResponse<Recipient>> GetGroupMembersAsync(
            int groupId,
            int pageNumber,
            int pageSize,
            string? search = null,
            bool? isActive = null);

        Task<PagedResultResponse<RecipientGroup>> GetRecipientGroupsAsync(
            int recipientId,
            int pageNumber,
            int pageSize);

        // Count Operations
        Task<int> CountGroupMembersAsync(int groupId);
        Task<int> CountRecipientGroupsAsync(int recipientId);

        // Membership History
        Task<PagedResultResponse<GroupMembershipRecord>> GetMembershipHistoryAsync(
            int groupId,
            int recipientId,
            int pageNumber,
            int pageSize);
    }

    public class GroupMembershipRecord
    {
        public int GroupId { get; set; }
        public int RecipientId { get; set; }
        public DateTime AddedAt { get; set; }
        public int AddedByUserId { get; set; }
    }
}