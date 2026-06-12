using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorDetailsController(IDoctorDetailsService _service) : ControllerBase
    {
        [HttpGet("{doctorId:guid}")]
        public async Task<IActionResult> GetDetailsOfDoctorById(Guid doctorId)
        {
            var doctors = await _service.GetByDoctorId(doctorId);
            if (doctors == null || !doctors.Any())
                return BadRequest("No doctors found for this category.");

            return Ok(doctors);
        }


        [HttpPost("rate")]
        public async Task<IActionResult> RateDoctor([FromBody] RateDoctorDto dto)
        {
            var result = await _service.Rating(dto.DoctorId,dto.rate);

            if (!result.Succeeded)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }


    }
}




