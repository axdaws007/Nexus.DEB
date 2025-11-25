namespace Nexus.DEB.Domain.Models.Other
{
	public class ModuleInfo
	{
		public Guid ModuleId { get; set; }
		public string ModuleName { get; set; } = string.Empty;
		public string? AssemblyName { get; set; }
		public string? IOCName { get; set; }
		public bool Enabled { get; set; }
		public string? SchemaName { get; set; }
		public bool IsIssueLinkable { get; set; }

		public virtual ICollection<ModuleSetting> ModuleSettings { get; set; }
	}
}
