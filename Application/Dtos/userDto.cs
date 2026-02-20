namespace Application.Dtos
{
    public class userDto
    {
        public required string Name { get; set; }
        public required string Nickname { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = "";
        public required string PhotoUrl { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string UserName { get; set; }


    }
}


