using Domain.Entities;

namespace Application.Interfaces
{
    public interface IJwt
    {
        Task<string> GenerateToken(User user);
    }
}
