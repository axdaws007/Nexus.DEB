using System.Text.Json;

namespace Nexus.DEB.Application.Common.Models.Dms
{
    public class DmsDocument
    {
        public string FileName { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public Guid DocumentId { get; set; }
        public Guid DocumentOwnerId { get; set; }
        public string? DocumentOwner { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public int? FileSize { get; set; }
        public string? Title { get; set; }
        public DateTime UploadedDate { get; set; }
        public string? UploadedBy { get; set; }
        public Dictionary<string, JsonElement>? Metadata { get; set; }

		[Obsolete("EntityId is now in Metadata dictionary")]
		public Guid? EntityId { get; set; }
	}
}
