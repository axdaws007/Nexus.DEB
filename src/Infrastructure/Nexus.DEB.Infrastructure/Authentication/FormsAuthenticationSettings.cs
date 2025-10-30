namespace Nexus.DEB.Infrastructure.Authentication
{
    /// <summary>
    /// Configuration settings for legacy .NET Framework 4.8 Forms Authentication
    /// </summary>
    public class FormsAuthenticationSettings
    {
        public const string SectionName = "FormsAuthentication";

        /// <summary>
        /// Decryption key (hex string) - must match legacy .NET 4.8 app
        /// </summary>
        public string DecryptionKey { get; set; } = string.Empty;

        /// <summary>
        /// Validation key (hex string) - must match legacy .NET 4.8 app
        /// </summary>
        public string ValidationKey { get; set; } = string.Empty;

        /// <summary>
        /// Cookie name - typically ".ASPXAUTH"
        /// </summary>
        public string CookieName { get; set; } = ".ASPXAUTH";

        /// <summary>
        /// Cookie expiration in minutes
        /// </summary>
        public int ExpirationMinutes { get; set; } = 480; // 8 hours default

        /// <summary>
        /// Whether the cookie should persist across browser sessions
        /// </summary>
        public bool PersistentCookie { get; set; } = false;

        /// <summary>
        /// Cookie domain (leave empty for default)
        /// </summary>
        public string? CookieDomain { get; set; }

        /// <summary>
        /// Cookie path - typically "/"
        /// </summary>
        public string CookiePath { get; set; } = "/";

        /// <summary>
        /// Require HTTPS for cookie transmission
        /// </summary>
        public bool RequireHttps { get; set; } = true;
    }
}
