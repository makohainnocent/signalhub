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


    }
}
