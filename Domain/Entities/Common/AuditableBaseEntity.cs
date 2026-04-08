
namespace Domain.Entities.Common
{
    public abstract class AuditableBaseEntity : BaseEntity
    {
        public string CreatedBy { get; set; } = null!;
        public DateTime Created { get; set; }
        public string LastModifiedBy { get; set; } = null!;
        public DateTime? LastModified { get; set; }
    }
}
