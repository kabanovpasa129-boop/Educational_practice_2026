using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechmartAPI.Data;
using TechmartAPI.DTOs;

namespace TechmartAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Login == loginDto.Login && u.Password == loginDto.Password);

            if (user == null)
                return Unauthorized(new { message = "Неверный логин или пароль" });

            return Ok(new UserDto
            {
                Id = user.Id,
                Login = user.Login,
                Role = user.Role
            });
        }
    }
}