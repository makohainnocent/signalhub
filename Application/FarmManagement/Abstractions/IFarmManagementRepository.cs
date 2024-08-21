using Domain.Authentication.Requests;
using Domain.Authentication.Responses;
using Domain.Common.Responses;
using Domain.Core.Models;
using Domain.FarmManagement.Requests;
using Domain.FarmManagement.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.FarmManagement.Abstractions
{
    public interface IFarmManagementRepository
    {

        Task<Farm> CreateFarm(FarmCreationRequest request, int userId);
        Task<PagedResultResponse<Farm>> GetAllFarmsAsync(int pageNumber, int pageSize, string? search = null);
        Task<Farm?> GetFarmByIdAsync(int farmId);
        Task<Farm> UpdateFarm(FarmUpdateRequest request);
        Task<bool> DeleteFarm(int farmId);
        Task<FarmGeofencing> CreateFarmGeofencing(FarmGeofencingRequest request);
        Task<PagedResultResponse<FarmGeofencingWithFarmDetailsResponse>> GetAllFarmGeofencingsAsync(int pageNumber, int pageSize, string? search = null);
        Task<FarmGeofencingWithFarmDetailsResponse?> GetMostRecentGeofenceByFarmIdAsync(int farmId);
        Task<FarmGeofencing> UpdateGeofenceAsync(FarmGeofencingUpdateRequest request);
        Task<bool> DeleteGeofenceAsync(int geofenceId);


    }
}
