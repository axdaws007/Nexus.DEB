using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Domain.Models
{
    public class StandardVersion : EntityHead
    {
        public StandardVersion() : base()
        {
            EntityTypeTitle = EntityTypes.StandardVersion;
        }

        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string Reference { get; set; }
        public int? MajorVersion { get; set; }
        public int? MinorVersion { get; set; }
        public bool UseVersionPrefix { get; set; }

        public short StandardId { get; set; }
        public virtual Standard Standard { get; set; }
    }
}
