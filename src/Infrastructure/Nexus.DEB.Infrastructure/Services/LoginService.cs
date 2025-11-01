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
        private readonly ICisService _userValidationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly string _authCookieName;

        public LoginService(
            ICisService userValidationService,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _userValidationService = userValidationService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;

            _authCookieName = _configuration["Authentication:CookieName"]
                ?? throw new InvalidOperationException("Authentication:CookieName is not configured");
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
            var cisUser = await _userValidationService.ValidateCredentialsAsync(username, password);

            if (cisUser == null)
            {
                return Result<LoginResponse>.Failure(new ValidationError
                {
                    Field = "credentials",
                    Message = "Invalid username or password",
                    Code = "INVALID_CREDENTIALS"
                });
            }

            Guid postId;
            List<CisPost>? postsToReturn = null;

            if (cisUser.Posts == null || cisUser.Posts.Count == 0)
            {
                // No posts assigned - use empty guid
                postId = Guid.Empty;
            }
            else if (cisUser.Posts.Count == 1)
            {
                // Single post - use that post's ID
                postId = cisUser.Posts[0].PostId;
            }
            else
            {
                // Multiple posts - use empty guid and return posts for selection
                postId = Guid.Empty;
                postsToReturn = cisUser.Posts;
            }

            // Create claims with the determined PostId and UserId
            var claims = new List<Claim>
            {
                new Claim("PostId", postId.ToString()),
                new Claim("UserId", cisUser.UserId.ToString()),
                new Claim(ClaimTypes.Name, $"{postId}|{cisUser.UserId}"),
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
                UserId = cisUser.UserId,
                PostId = postId,
                Username = username,
                Success = true,
                ExpiresAt = expiresUtc,
                Posts = postsToReturn  // Will be null unless multiple posts
            };

            return Result<LoginResponse>.Success(response);
        }

        public async Task<Result<SelectPostResponse>> SelectPostAsync(Guid postId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return Result<SelectPostResponse>.Failure(new ValidationError
                {
                    Field = "system",
                    Message = "Unable to access HTTP context",
                    Code = "HTTP_CONTEXT_ERROR"
                });
            }

            // Ensure user is authenticated
            if (!httpContext.User.Identity?.IsAuthenticated ?? true)
            {
                return Result<SelectPostResponse>.Failure(new ValidationError
                {
                    Field = "authentication",
                    Message = "User must be authenticated to select a post",
                    Code = "NOT_AUTHENTICATED"
                });
            }

            // Get the current UserId from the existing claims
            var userIdClaim = httpContext.User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Result<SelectPostResponse>.Failure(new ValidationError
                {
                    Field = "userId",
                    Message = "Unable to retrieve user ID from authentication cookie",
                    Code = "USERID_NOT_FOUND"
                });
            }

            // Validate postId
            if (postId == Guid.Empty)
            {
                return Result<SelectPostResponse>.Failure(new ValidationError
                {
                    Field = "postId",
                    Message = "Post ID cannot be empty",
                    Code = "INVALID_POSTID"
                });
            }

            var authCookie = httpContext.Request.Cookies[_authCookieName];
            var cookieHeader = $"{_authCookieName}={authCookie}";

            var isPostValid = await _userValidationService.ValidatePostAsync(userId, postId, cookieHeader);

            if (!isPostValid)
            {
                return Result<SelectPostResponse>.Failure(new ValidationError
                {
                    Field = "postId",
                    Message = "Post ID not valid for this user",
                    Code = "INVALID_POSTID"
                });
            }

            // Get existing authentication properties to preserve rememberMe and expiration
            var existingAuthResult = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var isPersistent = existingAuthResult.Properties?.IsPersistent ?? false;

            // Get cookie expiration from configuration (default: 480 minutes = 8 hours)
            var cookieExpirationMinutes = 480;
            if (int.TryParse(_configuration["Authentication:CookieExpirationMinutes"], out var configMinutes))
            {
                cookieExpirationMinutes = configMinutes;
            }

            var expiresUtc = DateTimeOffset.UtcNow.AddMinutes(cookieExpirationMinutes);

            // Create new claims with the selected PostId
            var claims = new List<Claim>
            {
                new Claim("PostId", postId.ToString()),
                new Claim("UserId", userId.ToString()),
                new Claim(ClaimTypes.Name, $"{postId}|{userId}"),
                new Claim(ClaimTypes.Authentication, "Forms")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = expiresUtc,
                AllowRefresh = true
            };

            // Sign in with the updated claims (this recreates the cookie)
            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                authProperties);

            // Return success response
            var response = new SelectPostResponse
            {
                UserId = userId,
                PostId = postId,
                Success = true,
                ExpiresAt = expiresUtc
            };

            return Result<SelectPostResponse>.Success(response);
        }
    }
}
