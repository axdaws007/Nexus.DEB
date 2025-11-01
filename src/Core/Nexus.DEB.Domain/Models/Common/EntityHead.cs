using Nexus.DEB.Domain.Interfaces;

namespace Nexus.DEB.Domain.Models.Common;

public abstract class EntityHead : IEntityHead
{
    protected EntityHead()
    {
        Id = Guid.NewGuid();
        CreatedDate = DateTime.Now;
        LastModifiedDate = DateTime.Now;
    }

    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SerialNumber { get; set; }
    public Guid OwnedById { get; set; }
    public Guid? OwnedByGroupId { get; set; }
    public Guid CreatedById { get; set; }
    public Guid LastModifiedById { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public bool IsRemoved { get; set; }
    public bool IsArchived { get; set; }
    public string EntityTypeTitle { get; set; } = string.Empty;
}

public struct EntityTypes
{
    public const string Requirement = "Requirement";
    public const string SoC = "Statement of Compliance";
    public const string Task = "Task";
    public const string Scope = "Scope";
    public const string Evidence = "Evidence";
    public const string StandardVersion = "Standard Version";
}

