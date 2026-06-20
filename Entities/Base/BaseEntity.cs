namespace WebMVC.Entities.Base
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }

        public DateTime? Updated { get; set; }
        public int? UpdateBy { get; set; }
    }
}
