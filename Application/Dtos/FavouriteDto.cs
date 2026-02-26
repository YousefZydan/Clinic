namespace Application.Dtos
{
    public class FavouriteDto
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";

        public string? CategoryName { get;  set; }
        public string? Name { get;  set; }
        public string? About { get;  set; }
        public string? PhotoUrl { get; set; }

    }
}


