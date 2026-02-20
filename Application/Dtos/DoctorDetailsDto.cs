namespace Application.Dtos
{
    public class DoctorDetailsDto
    {

        public string DoctorName { get; set; }
        public string DoctorCategoryName { get; set; }

        public string? PhotoUrl { get; set; }

        public string AboutMe { get; set; } = string.Empty;

        public int PatientsCount { get; set; }

        public int YearsOfExperience { get; set; }

        public double Rating { get; set; }

        public int ReviewsCount { get; set; }
        public ICollection<WorkingTimeDto> WorkingTimes { get; set; } = new List<WorkingTimeDto>();

    }
}


