using Domain.Entities;
using Domain.Premitives;

public class DoctorDetails : Audited
{
    public string AboutMe { get; set; } = string.Empty;

    public int PatientsCount { get; set; }

    public int YearsOfExperience { get; set; }

    public double Rating { get; set; }

    public int ReviewsCount { get; set; }

    public Guid DoctorId { get; set; }

    public Doctor Doctor { get; set; } = null!;

    public ICollection<WorkingTime> WorkingTimes { get; set; } = new List<WorkingTime>();
}

