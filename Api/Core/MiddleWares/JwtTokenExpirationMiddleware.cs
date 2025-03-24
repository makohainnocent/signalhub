using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Api.Core.MiddleWares
{
    public class JwtTokenExpirationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtTokenExpirationMiddleware> _logger;

        public JwtTokenExpirationMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<JwtTokenExpirationMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                if (!string.IsNullOrEmpty(token))
                {
                    // Validate the JWT token and check expiration
                    var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();

                    try
                    {
                        var validationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = false, // Do not validate lifetime here; we will handle it manually.
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = _configuration["JwtSettings:Issuer"],
                            ValidAudience = _configuration["JwtSettings:Audience"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]))
                        };

                        // Validate token (this will ensure signature and other checks, but not expiration)
                        var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                        var jwtToken = validatedToken as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;

                        if (jwtToken != null)
                        {
                            // Manually check if the token has expired
                            var expirationDate = jwtToken.ValidTo;
                            if (expirationDate < DateTime.UtcNow)
                            {
                                // Token is expired
                                context.Response.StatusCode = 419;
                                context.Response.ContentType = "application/json";
                                var result = System.Text.Json.JsonSerializer.Serialize(new
                                {
                                    StatusCode = 419,
                                    Message = "Your token has expired. Please login again."
                                });
                                await context.Response.WriteAsync(result);
                                return;
                            }
                        }
                    }
                    catch (SecurityTokenException ex)
                    {
                        // Log the exception for debugging
                        _logger.LogError(ex, "JWT Token validation failed.");

                        // Handle token validation failure
                        context.Response.StatusCode = 419;
                        context.Response.ContentType = "application/json";
                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            StatusCode = 419,
                            Message = "Invalid token. Please provide a valid token."
                        });
                        await context.Response.WriteAsync(result);
                        return;
                    }
                }
            }

            // Continue processing the request if token is valid
            await _next(context);
        }
    }

}
