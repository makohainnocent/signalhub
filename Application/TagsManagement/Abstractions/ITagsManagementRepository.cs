using Domain.Core.Models;
using Domain.Common.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.TagsManagement.Abstractions
{
    public interface ITagsManagementRepository
    {
        // Tag-related methods
        Task<Tag> CreateTagAsync(Tag tag);
        Task<Tag?> GetTagByIdAsync(int tagId);
        Task<PagedResultResponse<Tag>> GetTagsAsync(int pageNumber, int pageSize, string? search = null);
        Task<bool> UpdateTagAsync(Tag tag);
        Task<bool> DeleteTagAsync(int tagId);

        // TagApplication-related methods
        Task<TagApplication> CreateTagApplicationAsync(TagApplication application);
        Task<TagApplication?> GetTagApplicationByIdAsync(int applicationId);
        Task<PagedResultResponse<TagApplication>> GetTagApplicationsAsync(int pageNumber, int pageSize, int? applicantId, string? search = null,string?agent="no");
        Task<bool> UpdateTagApplicationAsync(TagApplication application);
        Task<bool> DeleteTagApplicationAsync(int applicationId);

        // TagIssuance-related methods
        Task<TagIssuance> CreateTagIssuanceAsync(TagIssuance issuance);
        Task<TagIssuance?> GetTagIssuanceByIdAsync(int issuanceId);
        Task<PagedResultResponse<TagIssuance>> GetTagIssuancesAsync(int pageNumber, int pageSize,int? issuedToId, string? search = null,string?agent="no");
        Task<bool> UpdateTagIssuanceAsync(TagIssuance issuance);
        Task<bool> DeleteTagIssuanceAsync(int issuanceId);
    }
}