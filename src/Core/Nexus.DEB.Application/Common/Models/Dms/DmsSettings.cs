namespace Nexus.DEB.Application.Common.Models.Dms
{
    public class DmsSettings
    {
        public int MaximumFileSizeInBytes { get; set; } = 131072000; // 125MB
        public ICollection<string> AllowedFileExtensions { get; set; } = [ ".doc", ".jpg", ".txt", ".docx", ".xls", ".xlsx", ".pdf", ".gif"];
    }
}
