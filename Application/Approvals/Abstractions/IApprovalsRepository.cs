using Domain.Approvals.Requests;
using Domain.Common.Responses;
using Domain.Core.Models;

namespace Application.Approvals.Abstractions
{
    public interface IApprovalsRepository
    {
        Task<Approval> CreateApprovalAsync(CreateApprovalRequest request);
        Task<Approval?> GetApprovalByIdAsync(int approvalId);
        Task<PagedResultResponse<Approval>> GetAllApprovalsAsync(int pageNumber, int pageSize, string? search = null);
        Task<Approval> UpdateApprovalAsync(Approval approval);
        Task<bool> DeleteApprovalAsync(int approvalId);
    }
}
