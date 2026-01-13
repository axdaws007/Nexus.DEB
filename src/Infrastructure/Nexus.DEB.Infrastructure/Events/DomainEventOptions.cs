using Microsoft.Extensions.Logging;

namespace Nexus.DEB.Infrastructure.Events
{
    public class DomainEventOptions
    {
        /// <summary>
        /// Logger for registration diagnostics (optional)
        /// </summary>
        public ILogger? Logger { get; set; }

        /// <summary>
        /// Assembly name prefixes to scan for subscribers. Default: ["Nexus."]
        /// </summary>
        public List<string> AssemblyPrefixes { get; set; } = new() { "Nexus." };
    }
}
