using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Infrastructure.Helpers
{
    public static class TokenParser
    {
        public static Result<(Guid PostId, Guid UserId)> ParseCookieToken(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return Result<(Guid PostId, Guid UserId)>.Failure(new ValidationError
                {
                    Field = "token",
                    Message = "Token is required",
                    Code = "TOKEN_REQUIRED"
                });
            }

            var parts = token.Split('|');

            if (parts.Length != 2)
            {
                return Result<(Guid PostId, Guid UserId)>.Failure(new ValidationError
                {
                    Field = "token",
                    Message = "Token format is invalid. Expected format: 'postId|userId'",
                    Code = "INVALID_TOKEN_FORMAT"
                });
            }

            if (!Guid.TryParse(parts[0], out var postId))
            {
                return Result<(Guid PostId, Guid UserId)>.Failure(new ValidationError
                {
                    Field = "token",
                    Message = "Invalid PostId in token",
                    Code = "INVALID_POSTID"
                });
            }

            if (!Guid.TryParse(parts[1], out var userId))
            {
                return Result<(Guid PostId, Guid UserId)>.Failure(new ValidationError
                {
                    Field = "token",
                    Message = "Invalid UserId in token",
                    Code = "INVALID_USERID"
                });
            }

            return Result<(Guid PostId, Guid UserId)>.Success((postId, userId));
        }
    }
}
