using Application.Dtos;
using Application.Interfaces;
using Clinic.Controllers;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class DoctorController(IDoctorServices _doctorService) : ApiController
    {

        [HttpGet("by-category/{categoryId:guid}")]
        public async Task<IActionResult> GetDoctorsByCategoryId(Guid categoryId)
        {
            var doctors = await _doctorService.GetDoctorByCategoryId(categoryId);
            if (doctors == null || !doctors.Any())
                return NotFound("No doctors found for this category.");

            return Ok(doctors);  
        }

        [HttpGet("by-CategoryName/{categoryName}")]
        public async Task<IActionResult> GetDoctorsByCategoryName(string categoryName)
        {
            var doctors = await _doctorService.GetDoctorByCategoryName(categoryName);
            if (doctors == null || !doctors.Any())
                return NotFound("No doctors found for this category.");

            return Ok(doctors);
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetAllDoctors()
        {
            var doctors = await _doctorService.GetAllDoctors();

            return Ok(doctors);

        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetDoctorsByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Name is required.");

            var doctors = await _doctorService.GetDoctorByName(name);
            if (doctors == null || !doctors.Any())
                return NotFound("No doctors found with that name.");

            return Ok(doctors);
        }




    }
}

