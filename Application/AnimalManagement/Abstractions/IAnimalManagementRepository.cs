using Application.Common.Abstractions;
using Domain.Common.Responses;
using Domain.Core.Models;
using Domain.AnimalManagement.Requests;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.AnimalManagement.Abstractions
{
    public interface IAnimalManagementRepository
    {
        // Animal-related methods
        Task<Animal> CreateAnimalAsync(AnimalCreationRequest request);
        Task<PagedResultResponse<Animal>> GetAnimalsByPremisesAsync(int premisesId, int pageNumber, int pageSize, string? search = null);
        Task<Animal?> GetAnimalByIdAsync(int animalId);
        Task<Animal?> UpdateAnimalAsync(AnimalUpdateRequest request);
        Task<bool> DeleteAnimalAsync(int animalId);
        Task<int> CountAnimalsAsync(int? ownerId = null, int? premisesId = null);

        // HealthRecord-related methods
        Task<HealthRecord> CreateHealthRecordAsync(HealthRecordCreationRequest request);
        Task<PagedResultResponse<HealthRecord>> GetHealthRecordsByAnimalIdAsync(int animalId, int pageNumber, int pageSize, string? search = null);
        Task<HealthRecord?> GetHealthRecordByIdAsync(int healthRecordId);
        Task<bool> UpdateHealthRecordAsync(int healthRecordId, UpdateHealthRecordRequest updateRequest);
        Task<int> CountHealthRecordsAsync(int? userId = null, int? animalId = null, int? premisesId = null);
        Task<PagedResultResponse<HealthRecord>> GetAllHealthRecordsAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            int? userId = null,
            int? animalId = null,
            int? premisesId = null);

        Task<bool> DeleteHealthRecordAsync(int healthRecordId); // Added method
    }
}