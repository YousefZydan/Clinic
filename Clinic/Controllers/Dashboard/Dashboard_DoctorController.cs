using Application.Dtos;
using Application.Interfaces;
using Application.Interfaces.Dashboard;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Clinic.Controllers.Dashboard
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class Dashboard_DoctorController(IDashboardDoctorService _dashboardService) : ControllerBase
    {

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDoctorDto input)
        {
            var result = await _dashboardService.Login(input);

            if (!result.Succeeded)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }


        [HttpGet("profileDoctor")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.GetUserId();
                if (string.IsNullOrEmpty(userId))
                        return Unauthorized("User not found");

            var user = await _dashboardService.GetProfileInfo(userId);

            if (user == null)
                return NotFound("User not found");

            return Ok(user);
        }


        [HttpPut("booking-status/{bookingId:Guid}")]
        public async Task<IActionResult> ChangeBookingStatus([FromRoute]Guid bookingId,[FromQuery] string status)
        {
                var userId = User.GetUserId();
                    if (string.IsNullOrEmpty(userId))
                            return Unauthorized("User not found");

            var result = await _dashboardService.BookingStatus(bookingId, status,userId);


            if (!result.Succeeded)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }


        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookings([FromQuery] string? status)
        {
            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized();

            var result = await _dashboardService.GetMyBookings(userId, status!);

            if (!result.Succeeded)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }

        [HttpPost("AddPrescription")]
        public async Task<IActionResult> AddPrescription([FromForm] PrescriptionCreateDto dto)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();


            var result = await _dashboardService.AddingPrescription(userId, dto);

            if (!result.Succeeded)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }



    }
}









