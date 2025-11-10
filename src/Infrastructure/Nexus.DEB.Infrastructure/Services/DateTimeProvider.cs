using Nexus.DEB.Application.Common.Interfaces;

namespace Nexus.DEB.Infrastructure.Services
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime Now => DateTime.UtcNow;
    }
}
