using System.ComponentModel.DataAnnotations;

namespace Domain.Premitives
{
    public class Base<T>
    {
        protected Base() { }

        [Key]
        public T Id { get; set; }

    }
}


