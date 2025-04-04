using Domain.Common.Responses;
using Domain.NotificationTemplates;
using Domain.NotificationTemplates.Requests;
using Domain.TemplateChannels;

namespace Application.NotificationTemplates.Abstractions
{
    public interface INotificationTemplatesRepository
    {
        // Basic CRUD Operations
        Task<NotificationTemplate> CreateTemplateAsync(NotificationTemplateCreationRequest request);
        Task<PagedResultResponse<NotificationTemplate>> GetTemplatesAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            int? applicationId = null,
            int? createdByUserId = null,
            string? approvalStatus = null,
            bool? isActive = null);
        Task<NotificationTemplate?> GetTemplateByIdAsync(int templateId);
        Task<NotificationTemplate> UpdateTemplateAsync(NotificationTemplateUpdateRequest request);
        Task<bool> DeleteTemplateAsync(int templateId);
        Task<int> CountTemplatesAsync();

        // Template Versioning
        Task<NotificationTemplate> CreateNewVersionAsync(int templateId, int createdByUserId);
        Task<PagedResultResponse<NotificationTemplate>> GetTemplateVersionsAsync(int templateId, int pageNumber, int pageSize);

        // Approval Workflow
        Task<bool> SubmitForApprovalAsync(int templateId, int submittedByUserId);
        Task<bool> ApproveTemplateAsync(int templateId, int approvedByUserId);
        Task<bool> RejectTemplateAsync(int templateId, int rejectedByUserId, string comments);
        Task<bool> RevertToDraftAsync(int templateId, int revertedByUserId);

        // Template Activation
        Task<bool> ActivateTemplateAsync(int templateId);
        Task<bool> DeactivateTemplateAsync(int templateId);

        // Template Content Operations
        Task<string> GetTemplateContentAsync(int templateId);
        Task<bool> UpdateTemplateContentAsync(int templateId, string content, int updatedByUserId);
        Task<bool> UpdateVariablesSchemaAsync(int templateId, string schemaJson, int updatedByUserId);

        // Channel Templates
        Task<PagedResultResponse<TemplateChannel>> GetTemplateChannelsAsync(int templateId, int pageNumber, int pageSize);
        Task<int> CountTemplateChannelsAsync(int templateId);
    }
}