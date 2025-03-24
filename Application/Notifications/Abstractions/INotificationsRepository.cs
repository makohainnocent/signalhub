// Application/Notifications/Abstractions/INotificationsRepository.cs
using Domain.Common.Responses;
using Domain.Notifications.Models;
using Domain.Notifications.Requests;

namespace Application.Notifications.Abstractions
{
    public interface INotificationsRepository
    {
        Task<Notification> CreateNotificationAsync(NotificationCreationRequest request);
        Task<PagedResultResponse<Notification>> GetNotificationsAsync(int pageNumber, int pageSize, string? search = null, int? userId=null,string? status=null);
        Task<Notification?> GetNotificationByIdAsync(int notificationId);
        Task<Notification> UpdateNotificationAsync(NotificationUpdateRequest request);
        Task<bool> DeleteNotificationAsync(int notificationId);
        Task<int> CountNotificationsAsync();
    }
}