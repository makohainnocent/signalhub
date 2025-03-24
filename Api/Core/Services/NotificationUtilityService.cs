using Application.Notifications.Abstractions;
using Domain.Notifications.Models;
using Domain.Notifications.Requests;
using Serilog;

namespace Api.Core.Services
{
    public class NotificationUtilityService
    {
        private readonly INotificationsRepository _notificationsRepository;

        public NotificationUtilityService(INotificationsRepository notificationsRepository)
        {
            _notificationsRepository = notificationsRepository;
        }

        public async Task<Notification> CreateNotificationAsync(int userId, string title, string message)
        {
            try
            {
                Log.Information("Creating notification for user ID: {UserId}", userId);

                // Create the notification request
                var request = new NotificationCreationRequest
                {
                    UserId = userId,
                    Title = title,
                    Body = message,
           
                };

                // Call the repository to create the notification
                var createdNotification = await _notificationsRepository.CreateNotificationAsync(request);

                Log.Information("Notification created successfully with ID: {NotificationId}", createdNotification.NotificationId);
                return createdNotification;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating the notification.");
                throw; // Re-throw the exception to be handled by the caller
            }
        }
    }
}
