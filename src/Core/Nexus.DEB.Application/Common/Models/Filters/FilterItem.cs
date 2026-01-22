using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Application.Common.Models.Filters
{
    public class FilterItemEntity
    {
        public Guid Id { get; set; }
        public string Value { get; set; }
        public bool IsEnabled { get; set; }
        public string? EntityType { get; set; }
	}

    public class FilterItem
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public bool IsEnabled { get; set; }
    }
}
