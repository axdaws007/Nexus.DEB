namespace Nexus.DEB.Application.Common.Models.Dms
{
    public class DmsDocumentActionData
    {
        public Guid ID { get; set; }

        public Guid ModuleID { get; set; }

        public Guid PostID { get; set; }

        public Guid? EntityID { get; set; }

        public Guid LibraryID { get; set; }

        public Guid AppInstanceID { get; set; }

        public Guid? DocumentTypeGroupID { get; set; }

        public bool CanEdit { get; set; }

        public bool CanDelete { get; set; }

        public bool CanView { get; set; }

        public bool CanViewHistory { get; set; }

        public string? AdditionalFieldsAction { get; set; }

        public string? AdditionalFieldsController { get; set; }

        public string? BaseUrl { get; set; }

        public bool IsNote { get; set; }
    }
}
