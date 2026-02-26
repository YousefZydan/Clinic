using Application.Dtos;
using Application.Interfaces.Dashboard;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        //[HttpGet("appointments")]
        //public async Task<IActionResult> GetAppointments([FromQuery] string status)
        //{
        //    var userId = User.GetUserId();
        //    if (string.IsNullOrEmpty(userId))
        //            return Unauthorized("User not found");

        //    var result = await _dashboardService.GetAppointments(userId, status);

        //    if (!result.Succeeded)
        //        return NotFound(result.Error);

        //    return Ok(result.Data);
        //}

        //[HttpPost("appointments/{appointmentId:guid}/complete")]
        //public async Task<IActionResult> CompleteAppointment([FromRoute] Guid appointmentId)
        //{
        //    var result = await _dashboardService.CompleteAppointment(appointmentId);

        //    if (!result.Succeeded)
        //        return BadRequest(result.Error);

        //    return Ok(result.Data);
        //}


        //[HttpPost("appointments/{appointmentId:guid}/cancel")]
        //public async Task<IActionResult> CancelAppointment([FromRoute] Guid appointmentId)
        //{
        //    var result = await _dashboardService.CancelAppointment(appointmentId);

        //    if (!result.Succeeded)
        //        return BadRequest(result.Error);

        //    return Ok(result.Data);
        //}

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
    }
}

