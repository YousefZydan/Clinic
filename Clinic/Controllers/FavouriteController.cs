using Application.Dtos;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FavouriteController : ControllerBase
    {
        private readonly IFavouriteService _service;

        public FavouriteController(IFavouriteService service)
        {
            _service = service;
        }

        [HttpPost("AddToFav")]
        public async Task<IActionResult> AddToFav([FromBody] FavouriteCreateDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();
            if(string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");

            var result = await _service.AddToFav(input,userId);

            if (!result.Succeeded)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }


        [HttpGet("GetFavourites")]
        public async Task<IActionResult> GetUserFavourites()
        {
            string userId = User.GetUserId()!;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");

            var favs = await _service.GetUserFavourites(userId);

            if (favs == null || favs.Count == 0)
                return NotFound("No favourite records found");

            return Ok(favs);
        }



        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Remove(Guid id)
        {
            string userId = User.GetUserId()!;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");

            var result = await _service.RemoveFromFav(id,userId);

            if (!result.Succeeded)
                return NotFound(result.Error);

            return Ok(result.Data);
        }
    }
}
