using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Application.Common.Models
{
    public class LinkedDocumentsForEntity
    {
        public required EntityHead EntityHead { get; set; }
        public ICollection<EntityDocumentLinking> LinkedDocuments { get; set; } = [];

    }
}
