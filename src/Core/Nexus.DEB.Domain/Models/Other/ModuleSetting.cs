namespace Nexus.DEB.Domain.Models.Other
{
    public class ModuleSetting
    {
        public Guid ModuleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Value { get; set; } = string.Empty;
        public int TypeId { get; set; }
        public virtual SettingsType Type { get; set; }
        public bool IsNullable { get; set; }
        public string? Description { get; set; }
        public bool IsCustomerSet { get; set; }

		public virtual ModuleInfo ModuleInfo { get; set; }
	}
}
