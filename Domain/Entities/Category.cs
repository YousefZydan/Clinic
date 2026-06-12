using Domain.Premitives;

namespace Domain.Entities
{
    public class Category : Audited<Guid>
    {
        private Category()
        {
            
        }

        public string Name { get;private set; }

        public Category(string name)
        {
            Name = name;
        }

        public List<Doctor> Doctors { get; set; } 


    }
}


