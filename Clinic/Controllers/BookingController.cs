using Application.Interfaces;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Clinic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingController(IBookingService _bookingService) : ControllerBase
    {

            [HttpPost("{workingTimeId}")]
            public async Task<IActionResult> Book([FromRoute] Guid workingTimeId)
            {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var result = await _bookingService.Booking(workingTimeId, userId);

                if (!result.Succeeded)
                    return BadRequest(result.Error);

                return Ok(result.Data);
            }


            [HttpDelete("{workingTimeId}")]
            public async Task<IActionResult> Cancel([FromRoute] Guid workingTimeId)
            {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var result = await _bookingService.CancelBooking(workingTimeId, userId);

                if (!result.Succeeded)
                    return BadRequest(result.Error);

                return Ok(result.Data);
            }


            [HttpGet]
            public async Task<IActionResult> GetMyBookings()
            {
              var userId = User.GetUserId();
              if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated.");

              var result = await _bookingService.GetAllBooking(userId);

              if (!result.Succeeded)
                 return BadRequest(result.Error);

 
              return Ok(result.Data);
           }
        }
    }
