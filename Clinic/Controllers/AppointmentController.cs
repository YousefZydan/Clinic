using Application.Dtos;
using Application.Interfaces;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentController(IAppointmentServices _service) : ControllerBase
    {
        [HttpPost("{doctorId}")]
        public async Task<IActionResult> AddBooking([FromRoute]Guid doctorId,[FromBody] AppoinmentCreate_EditDto dto)
        {
            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized();

            var result = await _service.AddToBooking(doctorId, userId, dto);

            if (!result.Succeeded)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }

        [HttpPut("cancel/{bookingId}")]
        public async Task<IActionResult> CancelBooking([FromRoute]Guid bookingId)
        {
            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized();

            var result = await _service.CancelBooking(bookingId, userId);

            if (!result.Succeeded)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }

        [HttpPut("{bookingId}")]
        public async Task<IActionResult> EditBooking(Guid bookingId,[FromBody] AppoinmentCreate_EditDto dto)
        {
            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized();

            var result = await _service.EditBooking(bookingId, userId, dto);

            if (!result.Succeeded)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }


        [HttpGet("MyBooking")]
        public async Task<IActionResult> GetUserBookings([FromQuery] string status)
        {
            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized();

            var result = await _service.GetUserBookings(userId, status);

            if (!result.Succeeded)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }

        [HttpGet("non-available/{doctorId:guid}")]
        public async Task<IActionResult> GetNonAvailableAppointments(Guid doctorId)
        {
            var result = await _service.NonAvailableAppointments(doctorId);

            if (!result.Succeeded)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }
    }
}