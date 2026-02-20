using Domain.Premitives;

namespace Domain.Entities
{
    public class Patient : Audited
    {
        private Patient()
        {
            
        }
        public Patient(string name, string userId)
        {
            Name = name;
            UserId = userId;
        }
        public string Name { get; private set; }
        public string UserId { get; private set; }
        public User User { get; private set; }
    }
}

