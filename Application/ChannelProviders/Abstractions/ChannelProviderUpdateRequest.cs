using Domain.Common.Responses;
using Domain.ChannelProviders.Requests;
using Domain.ChannelProviders;

namespace Application.ChannelProviders.Abstractions
{
    public interface IChannelProvidersRepository
    {
        // Provider CRUD Operations
        Task<ChannelProvider> CreateProviderAsync(ChannelProviderCreationRequest request);
        Task<PagedResultResponse<ChannelProvider>> GetProvidersAsync(
            int pageNumber,
            int pageSize,
            int? tenantId = null,
            string? channelType = null,
            string? search = null,
            bool? isActive = null,
            bool? isDefault = null);
        Task<ChannelProvider?> GetProviderByIdAsync(int providerId);
        Task<ChannelProvider> UpdateProviderAsync(ChannelProviderUpdateRequest request);
        Task<bool> DeleteProviderAsync(int providerId);
        Task<int> CountProvidersAsync();

        // Provider Configuration
        Task<bool> UpdateProviderConfigurationAsync(
            int providerId,
            string configurationJson,
            int updatedByUserId);
        Task<string> GetProviderConfigurationAsync(int providerId);

        // Default Provider Management
        Task<bool> SetAsDefaultProviderAsync(int providerId);
        Task<ChannelProvider?> GetDefaultProviderAsync(int tenantId, string channelType);

        // Priority Management
        Task<bool> UpdateProviderPriorityAsync(int providerId, int newPriority);
        Task<List<ChannelProvider>> GetProvidersByPriorityAsync(int tenantId, string channelType);

        // Status Management
        Task<bool> ActivateProviderAsync(int providerId);
        Task<bool> DeactivateProviderAsync(int providerId);

        // Validation
        Task<bool> ProviderNameExistsAsync(int tenantId, string name);
        Task<bool> HasActiveProvidersAsync(int tenantId, string channelType);
    }
}