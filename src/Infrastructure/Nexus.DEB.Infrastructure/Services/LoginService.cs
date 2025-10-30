using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using System.Security.Claims;

namespace Nexus.DEB.Infrastructure.Services
{
    public class LoginService : ILoginService
    {
        private readonly IUserValidationService _userValidationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public LoginService(
            IUserValidationService userValidationService,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _userValidationService = userValidationService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public async Task<Result<LoginResponse>> SignInAsync(string username, string password, bool rememberMe = false)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(username))
            {
                return Result<LoginResponse>.Failure(new ValidationError
                {
                    Field = "username",
                    Message = "Username is required",
                    Code = "USERNAME_REQUIRED"
                });
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return Result<LoginResponse>.Failure(new ValidationError
                {
                    Field = "password",
                    Message = "Password is required",
                    Code = "PASSWORD_REQUIRED"
                });
            }

            // Validate credentials using existing service
            var (isValid, userId, postId) = await _userValidationService.ValidateCredentialsAsync(username, password);

            if (!isValid)
            {
                return Result<LoginResponse>.Failure(new ValidationError
                {
                    Field = "credentials",
                    Message = "Invalid username or password",
                    Code = "INVALID_CREDENTIALS"
                });
            }

            // Create claims
            var claims = new List<Claim>
        {
            new Claim("PostId", postId.ToString()),
            new Claim("UserId", userId.ToString()),
            new Claim(ClaimTypes.Name, $"{postId}|{userId}"),
            new Claim(ClaimTypes.Authentication, "Forms")
        };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Get cookie expiration from configuration (default: 480 minutes = 8 hours)
            var cookieExpirationMinutes = 480;
            if (int.TryParse(_configuration["Authentication:CookieExpirationMinutes"], out var configMinutes))
            {
                cookieExpirationMinutes = configMinutes;
            }

            var expiresUtc = DateTimeOffset.UtcNow.AddMinutes(cookieExpirationMinutes);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = expiresUtc,
                AllowRefresh = true
            };

            // Create authentication ticket
            var ticket = new AuthenticationTicket(
                claimsPrincipal,
                authProperties,
                CookieAuthenticationDefaults.AuthenticationScheme);

            // Sign in the user (this will set the cookie automatically)
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return Result<LoginResponse>.Failure(new ValidationError
                {
                    Field = "system",
                    Message = "Unable to access HTTP context",
                    Code = "HTTP_CONTEXT_ERROR"
                });
            }

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                authProperties);

            // Return success response
            var response = new LoginResponse
            {
                UserId = userId,
                PostId = postId,
                Username = username,
                Success = true,
                ExpiresAt = expiresUtc
            };

            return Result<LoginResponse>.Success(response);
        }
    }
}
