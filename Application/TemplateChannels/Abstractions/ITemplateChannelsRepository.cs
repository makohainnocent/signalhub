using Domain.Common.Responses;
using Domain.NotificationTemplates.Requests;
using Domain.TemplateChannels.Requests;
using Domain.TemplateChannels;

namespace Application.NotificationTemplates.Abstractions
{
    public interface ITemplateChannelsRepository
    {
        // Channel CRUD Operations
        Task<TemplateChannel> CreateTemplateChannelAsync(TemplateChannelCreationRequest request);
        Task<TemplateChannel?> GetTemplateChannelByIdAsync(int templateChannelId);
        Task<TemplateChannel> UpdateTemplateChannelAsync(TemplateChannelUpdateRequest request);
        Task<bool> DeleteTemplateChannelAsync(int templateChannelId);

        // Template Channel Management
        Task<PagedResultResponse<TemplateChannel>> GetChannelsByTemplateAsync(
            int templateId,
            int pageNumber,
            int pageSize,
            string? channelType = null,
            bool? isActive = null);

        Task<TemplateChannel?> GetChannelByTypeAsync(int templateId, string channelType);

        // Content Management
        Task<bool> UpdateChannelContentAsync(
            int templateChannelId,
            string channelContentJson,
            int updatedByUserId);

        // Status Management
        Task<bool> ActivateChannelAsync(int templateChannelId);
        Task<bool> DeactivateChannelAsync(int templateChannelId);

        // Validation
        Task<bool> ChannelTypeExistsForTemplateAsync(int templateId, string channelType);

        // Count Operations
        Task<int> CountChannelsByTemplateAsync(int templateId);
        Task<int> CountActiveChannelsByTemplateAsync(int templateId);
    }
}