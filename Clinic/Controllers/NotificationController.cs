using Application.Interfaces;
using Application.Interfaces.Firebase;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;
        private readonly IFcmTokenService _fcmTokenService;

        public NotificationController(
            INotificationService service,
            IFcmTokenService fcmTokenService)
        {
            _service = service;
            _fcmTokenService = fcmTokenService;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not found" });

            var result = await _service.GetNotifications(userId);

            if (!result.Succeeded)
                return NotFound(new { error = result.Error });

            return Ok(result.Data);
        }

        [HttpPut("mark-as-read/{notificationId:guid}")]
        public async Task<IActionResult> MarkAsRead(Guid notificationId)
        {
            var result = await _service.MarkNotificationAsRead(notificationId);

            if (!result.Succeeded)
                return NotFound(new { error = result.Error });

            return Ok(result.Data);
        }

        [HttpPost("save-token")]
        public async Task<IActionResult> SaveToken([FromBody] SaveFcmTokenDto input)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not found" });

            await _fcmTokenService.SaveTokenAsync(userId, input.Token);

            return Ok(new { message = "FCM Token Saved Successfully" });
        }
    }

    public class SaveFcmTokenDto
    {
        public string Token { get; set; } = string.Empty;
    }
}