using Microsoft.AspNetCore.Mvc;
using TechmartAPI.Data;
using TechmartAPI.Models;

namespace TechmartAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ViewHistoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ViewHistoryController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddView([FromBody] ViewHistory viewHistory)
        {
            viewHistory.ViewDate = DateTime.Now;
            _context.ViewHistories.Add(viewHistory);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}