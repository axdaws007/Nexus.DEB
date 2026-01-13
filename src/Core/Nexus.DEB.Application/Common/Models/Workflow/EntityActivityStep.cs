namespace Nexus.DEB.Application.Common.Models
{
    public class EntityActivityStep
    {
        public int EntityActivityStepID { get; set; }

        public Guid EntityID { get; set; }

        public int ActivityID { get; set; }

        public string Title { get; set; }

        public int StatusID { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public Guid? UpdatedByID { get; set; }

        public Guid? OnBehalfOffID { get; set; }

        public Guid? SignatureID { get; set; }

        public Guid? RecipientSignatureID { get; set; }

        public string Comments { get; set; }

        public DateTime Created { get; set; }

        public Guid CreationGroupID { get; set; }

        public bool IsActive { get; set; }

        public List<string> LstCancelledReasons { get; set; }

        public Guid? UndoneBySignatureID { get; set; }

        public DateTime? UndoneDate { get; set; }
    }
}
