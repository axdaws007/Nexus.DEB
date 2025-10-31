﻿using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Domain.Models
{
    public class Scope : EntityHead
    {
        public DateTime? TargetImplementationDate { get; set; }
        public virtual ICollection<Requirement> Requirements { get; set; }
    }
}
