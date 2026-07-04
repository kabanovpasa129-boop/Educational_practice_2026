using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechmartAPI.Data;
using TechmartAPI.DTOs;

namespace TechmartAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Login = u.Login,
                    Role = u.Role
                })
                .ToListAsync();
            return Ok(users);
        }
    }
}