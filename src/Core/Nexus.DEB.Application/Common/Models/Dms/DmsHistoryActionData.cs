namespace Nexus.DEB.Application.Common.Models.Dms
{
    public class DmsHistoryActionData
    {
        public Guid DocID { get; set; }
        public Guid LibraryID { get; set; }
        public bool IsFileModified { get; set; }
        public int FileVersion { get; set; }
    }
}
