using Domain.Entities;

namespace Application.Dtos
{
    public class AppoinmentCreate_EditDto
    {
        public DateOnly Date { get; set; }
        public TimeOnly Hour { get; set; }

    }
}
