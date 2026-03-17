namespace Nexus.DEB.Domain.Models
{
    public struct ComplianceNodeTypes
    {
        public const string StandardVersion = "StandardVersion";
        public const string Section = "Section";
        public const string Requirement = "Requirement";
        public const string Statement = "Statement";
    }

    // Domain/Models/Compliance/BubbleUpQuantifier.cs
    public struct BubbleUpQuantifier
    {
        public const string Any = "Any";
        public const string All = "All";
    }

    // Domain/Models/Compliance/NodeDefaultScenario.cs
    public struct NodeDefaultScenario
    {
        public const string NoChildren = "NoChildren";
        public const string NoRuleMatch = "NoRuleMatch";
    }
}
