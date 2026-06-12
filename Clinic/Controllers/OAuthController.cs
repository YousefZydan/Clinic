using Application.Interfaces;
using Domain.Entities;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Controllers
{
    [Route("api/[controller]")]
    public class OAuthController : ApiController
    {
        private readonly IOAuthService _auth;

        public OAuthController(IOAuthService auth)
        {
            _auth = auth;
        }

        // POST: api/OAuth/google
        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
        {
            if (string.IsNullOrEmpty(dto.IdToken))
                return BadRequest("IdToken is required");

            GoogleJsonWebSignature.Payload payload;

            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken);
            }
            catch
            {
                return Unauthorized("Invalid Google token");
            }

            var email = payload.Email;
            var name = payload.Name;

            if (string.IsNullOrEmpty(email))
                return BadRequest("Email not found in Google token");

            var loginResult = await _auth.HandleExternalLoginAsync(email,name);

            if (!loginResult.Succeeded)
                return BadRequest(new { error = loginResult.Error });

            return Ok(new
            {
                token = loginResult.Data,
                email,
                name,
            });
        }
    }

    // DTO
    public class GoogleLoginDto
    {
        public string IdToken { get; set; } = null!;
    }
}
