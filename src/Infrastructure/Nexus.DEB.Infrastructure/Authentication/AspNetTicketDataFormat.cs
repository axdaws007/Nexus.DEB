using AspNetCore.LegacyAuthCookieCompat;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Nexus.DEB.Infrastructure.Authentication
{
    public class AspNetTicketDataFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private readonly LegacyFormsAuthenticationTicketEncryptor _encryptor;

        public AspNetTicketDataFormat(string decryptionKey, string validationKey)
        {
            byte[] decryptionKeyBytes = HexUtils.HexToBinary(decryptionKey);
            byte[] validationKeyBytes = HexUtils.HexToBinary(validationKey);

            _encryptor = new LegacyFormsAuthenticationTicketEncryptor(
                decryptionKeyBytes,
                validationKeyBytes,
                ShaVersion.Sha1);
        }

        public string Protect(AuthenticationTicket data)
        {
            return Protect(data, null);
        }

        public string Protect(AuthenticationTicket data, string? purpose)
        {
            if (data?.Principal?.Identity == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            // Extract PostId and UserId from claims
            var postIdClaim = data.Principal.FindFirst("PostId");
            var userIdClaim = data.Principal.FindFirst("UserId");

            if (postIdClaim == null || userIdClaim == null)
            {
                throw new InvalidOperationException("PostId and UserId claims are required");
            }

            // Create the pipe-separated username format: PostId|UserId
            string username = $"{postIdClaim.Value}|{userIdClaim.Value}";

            // Create legacy Forms Authentication ticket
            var expirationUtc = data.Properties.ExpiresUtc ?? DateTimeOffset.UtcNow.AddHours(8);

            // Use FormsAuthenticationTicket from AspNetCore.LegacyAuthCookieCompat library
            var legacyTicket = new FormsAuthenticationTicket(
                version: 2,
                name: username,
                issueDate: DateTime.UtcNow,
                expiration: expirationUtc.UtcDateTime,
                isPersistent: data.Properties.IsPersistent,
                userData: string.Empty,
                cookiePath: "/"
            );

            // Encrypt the ticket
            string encryptedTicket = _encryptor.Encrypt(legacyTicket);
            return encryptedTicket;
        }

        public AuthenticationTicket? Unprotect(string? protectedText)
        {
            return Unprotect(protectedText, null);
        }

        public AuthenticationTicket? Unprotect(string? protectedText, string? purpose)
        {
            if (string.IsNullOrEmpty(protectedText))
            {
                return null;
            }

            try
            {
                var formsAuthTicket = _encryptor.DecryptCookie(protectedText);

                if (formsAuthTicket == null)
                {
                    return null;
                }

                var ids = formsAuthTicket.Name.Split('|');

                if (ids.Length != 2)
                {
                    return null;
                }

                ClaimsIdentity claimsIdentity = new ClaimsIdentity(new[]
                {
                new Claim("PostId", ids[0]),
                new Claim("UserId", ids[1]),
                new Claim(ClaimTypes.Name, formsAuthTicket.Name),
                new Claim(ClaimTypes.Authentication, "Forms")
            }, CookieAuthenticationDefaults.AuthenticationScheme);

                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                var properties = new AuthenticationProperties
                {
                    ExpiresUtc = formsAuthTicket.Expiration,
                    IsPersistent = formsAuthTicket.IsPersistent
                };

                AuthenticationTicket ticket = new AuthenticationTicket(
                    claimsPrincipal,
                    properties,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return ticket;
            }
            catch
            {
                return null;
            }
        }
    }
}
