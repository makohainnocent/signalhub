using Domain.Authentication.Requests;
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


    }
}
