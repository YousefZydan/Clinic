using Domain.Entities;
using Domain.Premitives;
using System.ComponentModel.DataAnnotations.Schema;

public class DoctorDetails : Audited<Guid>
{
    public string AboutMe { get; set; } = string.Empty;

    public int PatientsCount { get; set; }

    public int YearsOfExperience { get; set; }

    public double Rating { get; set; }

    public int ReviewsCount { get; set; }
    [ForeignKey("Doctor")]

    public Guid DoctorId { get; set; }

    public Doctor Doctor { get; set; } = null!;

    public ICollection<WorkingTime> WorkingTimes { get; set; } = new List<WorkingTime>();
}

