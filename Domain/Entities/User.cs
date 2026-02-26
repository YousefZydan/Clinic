using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class User : IdentityUser
    {
        public required string Name { get; set; }
        public required string Nickname { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string? PhotoUrl { get; set; }
        public string? PhotoPublicId { get; set; }


        public List<Doctor> Doctors { get; set; } = new();
        public List<Patient> Patients { get; set; } = new();
        public List<Notification> Notifications { get; set; } = new();

        


    }
}




