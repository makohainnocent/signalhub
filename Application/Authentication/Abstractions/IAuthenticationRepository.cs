using Domain.Authentication.Requests;
using Domain.Authentication.Responses;
using Domain.Common.Responses;
using Domain.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Authentication.Abstractions
{
    public interface IAuthenticationRepository
    {
       
        Task<User> CreateUser(UserRegistrationRequest request);

        Task<User> LoginUser(UserLoginRequest request);

        Task<IEnumerable<Role>> GetRolesByUserIdAsync(int userId);

        Task<IEnumerable<Claim>> GetClaimsByUserIdAsync(int userId);

        Task<User> GetUserByIdAsync(int userId);

        Task<RefreshToken> GetByTokenAsync(string token);
        Task AddAsync(RefreshToken refreshToken);
        Task RevokeAsync(string token);
        Task StoreVerificationCode(string email, string code);
        Task<bool> ValidateVerificationCode(string email, string code);
        Task UpdatePassword(string email, string newPassword);
        Task<bool> ValidatePassword(string email, string password);
        Task UpdateUser(int userId, UpdateUserRequest request);
        Task<User> GetUserByUsernameAsync(string username);
        Task<PagedResultResponse<User>> GetUsersAsync(int pageNumber, int pageSize, string? search = null);
        Task DeleteUserAsync(int userId);
        Task<int> AddRoleAsync(Role newRole);
        Task<Role?> UpdateRoleAsync(UpdateRoleRequest roleRequest);
        Task<PagedResultResponse<Role>> GetRolesAsync(int page, int pageSize, string? search = null);
        Task<int> AddClaimAsync(AddClaimRequest claim);
        Task<Claim?> UpdateClaimAsync(UpdateClaimRequest request);
        Task<PagedResultResponse<Claim>> GetClaimsAsync(int pageNumber, int pageSize, string? search = null);
        Task<Claim?> GetClaimByIdAsync(int claimId);
        Task DeleteClaimAsync(int claimId);
        Task DeleteRoleAsync(int roleId);
        Task<Role?> GetRoleAsync(int roleId);
        Task<bool> AddRoleToUserAsync(AddRoleToUserRequest request);
        Task<bool> RemoveRoleFromUserAsync(RemoveRoleFromUserRequest request);
        Task<PagedResultResponse<UserRoleResponse>> GetUserRolesAsync(int pageNumber, int pageSize, string? search = null);
        Task<bool> AddClaimToRoleAsync(AddClaimToRoleRequest request);
        Task<bool> RemoveClaimFromRoleAsync(RemoveClaimFromRoleRequest request);
        Task<IEnumerable<RoleClaimResponse>> GetClaimsByRoleIdAsync(int roleId);
        Task<PagedResultResponse<RoleClaimResponse>> GetRoleClaimsAsync(int pageNumber, int pageSize, string? search = null);
        Task<bool> AddClaimToUserAsync(AddClaimToUserRequest request);
        Task<bool> RemoveClaimFromUserAsync(RemoveClaimFromUserRequest request);
        Task<PagedResultResponse<UserClaimResponse>> GetUserClaimsAsync(int pageNumber, int pageSize, string? search = null);


    }
}
