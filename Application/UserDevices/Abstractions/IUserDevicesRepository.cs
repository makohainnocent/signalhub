// Application/UserDevices/Abstractions/IUserDevicesRepository.cs
using Domain.Common.Responses;
using Domain.UserDevices.Models;
using Domain.UserDevices.Requests;

namespace Application.UserDevices.Abstractions
{
    public interface IUserDevicesRepository
    {
        Task<UserDevice> CreateUserDeviceAsync(UserDeviceCreationRequest request);
        Task<PagedResultResponse<UserDevice>> GetUserDevicesAsync(int pageNumber, int pageSize, string? search = null);
        Task<UserDevice?> GetUserDeviceByIdAsync(int deviceId);
        Task<UserDevice> UpdateUserDeviceAsync(UserDeviceUpdateRequest request);
        Task<bool> DeleteUserDeviceAsync(int deviceId);
        Task<int> CountUserDevicesAsync();
    }
}