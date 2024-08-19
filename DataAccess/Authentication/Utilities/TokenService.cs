using Application.Authentication.Abstractions;
using Domain.Core.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Authentication.Utilities
{
    public class TokenService
    {
        private readonly string _secretKey;
        private readonly IAuthenticationRepository _repo;
        private readonly string _issuer;
        private readonly string _audience;

        public TokenService(string secretKey, string issuer, string audience, IAuthenticationRepository repo)
        {
            _secretKey = secretKey;
            _issuer = issuer;
            _audience = audience;
            _repo = repo;
        }

        public async Task<string> GenerateTokenAsync(int userId)
        {
            // Retrieve user information from repositories
            var user = await _repo.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            var roles = await _repo.GetRolesByUserIdAsync(userId);
            var claims = await _repo.GetClaimsByUserIdAsync(userId);

            // Create claims for JWT
            var claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(ClaimTypes.Name, user.Username));
            claimsIdentity.AddClaims(roles.Select(role => new System.Security.Claims.Claim(ClaimTypes.Role, role.RoleName)));
            claimsIdentity.AddClaims(claims.Select(c => new System.Security.Claims.Claim(c.ClaimType, c.ClaimValue)));

            // Create JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = DateTime.UtcNow.AddHours(1), // Token expiration time
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<string> GenerateRefreshTokenAsync(int userId)
        {
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = Guid.NewGuid().ToString(),
                ExpiresAt = DateTime.UtcNow.AddDays(7), // Refresh token valid for 7 days
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(refreshToken);
            return refreshToken.Token;
        }
    }
}
