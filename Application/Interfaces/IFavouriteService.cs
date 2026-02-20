using Application.Dtos;
using Application.Helpers;

namespace Application.Interfaces
{
    public interface IFavouriteService 
    {
        Task<Result<string>> AddToFav(FavouriteCreateDto input,string userId);
        Task<Result<string>> RemoveFromFav(Guid Id);
        Task<List<FavouriteDto>> GetUserFavourites(string UserId);
        
    }
}




