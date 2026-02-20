using System.ComponentModel.DataAnnotations;

namespace Domain.Premitives
{
    public class Base
    {
        protected Base() { }

        [Key]
        public Guid Id { get; set; }

    }
}


