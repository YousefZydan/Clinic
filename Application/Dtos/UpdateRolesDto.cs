namespace Application.Dtos
{
    public class UpdateRolesDto
    {
        public string UserId { get; set; } = default!;
        public List<string> Roles { get; set; } = new();
    }
}


