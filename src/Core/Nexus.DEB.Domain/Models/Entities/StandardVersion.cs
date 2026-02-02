using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Domain.Models
{
    public class StandardVersion : EntityHead
    {
        public StandardVersion() : base()
        {
            EntityTypeTitle = EntityTypes.StandardVersion;
        }

        public DateOnly EffectiveStartDate { get; set; }
        public DateOnly? EffectiveEndDate { get; set; }
        public string Delimiter { get; set; } = string.Empty;
        public int? MajorVersion { get; set; }
        public int? MinorVersion { get; set; }

        public short StandardId { get; set; }
        public virtual Standard Standard { get; set; }

        public ICollection<Requirement> Requirements { get; set; }

        public string VersionTitle { get; set; } = string.Empty;
    }
}
