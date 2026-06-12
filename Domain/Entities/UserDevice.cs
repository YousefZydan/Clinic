using Domain.Entities;

public class UserDevice
{

    public int Id { get; set; }
    public string? UserId { get; set; }
    public User? User { get; set; }
    public string? FcmToken { get; set; }


}




