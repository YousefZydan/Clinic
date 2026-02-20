using Application.Dtos;
using Application.Interfaces;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class NotificationController(INotifaicationService _service) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> AddNotification([FromBody] NotificationCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.AddNotification(dto);

            if (!result.Succeeded)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            string? userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");

            var result = await _service.GetNotifications(userId);

            if (!result.Succeeded)
                return NotFound(result.Error);

            return Ok(result.Data);
        }
    }
}
