namespace UserManagement.Models
{
    public class BaseEntity 
    {
        public DateTime CreatedAt { get;  } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public Guid? CreatedBy { get; set; }
    }
}
