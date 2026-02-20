using Domain.Premitives;


namespace Domain.Entities
{
    public class Doctor : Audited
    {
        private Doctor()
        {
            
        }

        public string Name { get;private set; }
        public string About { get;private set; }

        public Doctor(string name,string about,string userId,Guid categoryId)
        {
            Name = name;
            About = about;
            UserId = userId;
            CategoryId = categoryId;
        }

        public string UserId { get;private set; }
        public User User { get; private set; }

        public Guid CategoryId { get; private set; }
        public Category Category { get; private set; }
        public List<DoctorDetails> DoctorDetails { get; set; }

        

    }
}

