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
    }
}


