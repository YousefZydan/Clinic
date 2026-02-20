namespace Domain.Premitives
{
    public class Audited : Base
    {
        protected Audited() { }

        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }


    }
}



