using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechmartAPI.Data;
using TechmartAPI.DTOs;
using TechmartAPI.Models;

namespace TechmartAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // ==================== УПРАВЛЕНИЕ ТОВАРАМИ ====================

        // GET: api/Admin/products
        [HttpGet("products")]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _context.Products
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Price = p.Price
                })
                .ToListAsync();
            return Ok(products);
        }

        // POST: api/Admin/product
        [HttpPost("product")]
        public async Task<IActionResult> AddProduct([FromBody] ProductDto productDto)
        {
            if (string.IsNullOrEmpty(productDto.Name) || string.IsNullOrEmpty(productDto.Category) || productDto.Price <= 0)
            {
                return BadRequest(new { message = "Некорректные данные товара" });
            }

            var product = new Product
            {
                Name = productDto.Name,
                Category = productDto.Category,
                Price = productDto.Price
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Товар добавлен", id = product.Id });
        }

        // PUT: api/Admin/product/{id}
        [HttpPut("product/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDto productDto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Товар не найден" });
            }

            product.Name = productDto.Name;
            product.Category = productDto.Category;
            product.Price = productDto.Price;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Товар обновлён" });
        }

        // DELETE: api/Admin/product/{id}
        [HttpDelete("product/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Товар не найден" });
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Товар удалён" });
        }

        // ==================== СТАТИСТИКА ====================

        // GET: api/Admin/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalProducts = await _context.Products.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();
            var totalRecommendations = await _context.Recommendations.CountAsync();

            var topProducts = await _context.OrderItems
                .GroupBy(oi => oi.ProductId)
                .Select(g => new { ProductId = g.Key, Count = g.Sum(oi => oi.Quantity) })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .Join(_context.Products, x => x.ProductId, p => p.Id, (x, p) => new
                {
                    p.Name,
                    p.Category,
                    SalesCount = x.Count
                })
                .ToListAsync();

            return Ok(new
            {
                totalProducts,
                totalUsers,
                totalOrders,
                totalRecommendations,
                topProducts
            });
        }

        // ==================== НАСТРОЙКА ВЕСОВ ====================

        // GET: api/Admin/weights
        [HttpGet("weights")]
        public async Task<IActionResult> GetWeights()
        {
            var settings = await _context.Settings.ToListAsync();
            var collaborative = settings.FirstOrDefault(s => s.Key == "ВесКоллаборативнойФильтрации")?.Value ?? 0.40m;
            var contentBased = settings.FirstOrDefault(s => s.Key == "ВесКонтентнойФильтрации")?.Value ?? 0.35m;
            var association = settings.FirstOrDefault(s => s.Key == "ВесПравилАссоциации")?.Value ?? 0.25m;

            return Ok(new UpdateWeightsDto
            {
                CollaborativeWeight = collaborative,
                ContentBasedWeight = contentBased,
                AssociationWeight = association
            });
        }

        // PUT: api/Admin/weights
        [HttpPut("weights")]
        public async Task<IActionResult> UpdateWeights([FromBody] UpdateWeightsDto weights)
        {
            var total = weights.CollaborativeWeight + weights.ContentBasedWeight + weights.AssociationWeight;
            if (Math.Abs(total - 1.0m) > 0.01m)
            {
                return BadRequest(new { message = "Сумма весов должна быть равна 1.0 (100%)" });
            }

            await UpdateSetting("ВесКоллаборативнойФильтрации", weights.CollaborativeWeight);
            await UpdateSetting("ВесКонтентнойФильтрации", weights.ContentBasedWeight);
            await UpdateSetting("ВесПравилАссоциации", weights.AssociationWeight);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Веса алгоритмов обновлены" });
        }

        private async Task UpdateSetting(string key, decimal value)
        {
            var setting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == key);
            if (setting == null)
            {
                setting = new Setting { Key = key, Value = value };
                _context.Settings.Add(setting);
            }
            else
            {
                setting.Value = value;
            }
        }

        // ==================== УПРАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯМИ ====================

        // GET: api/Admin/users
        [HttpGet("users")]
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

        // PUT: api/Admin/users/{id}/role
        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] string role)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Пользователь не найден" });
            }

            if (role != "покупатель" && role != "администратор")
            {
                return BadRequest(new { message = "Роль должна быть 'покупатель' или 'администратор'" });
            }

            user.Role = role;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Роль пользователя обновлена" });
        }

        // DELETE: api/Admin/users/{id}
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Пользователь не найден" });
            }

            // Защита от удаления главного администратора
            if (user.Login == "admin1")
            {
                return BadRequest(new { message = "Нельзя удалить главного администратора" });
            }

            var hasOrders = await _context.Orders.AnyAsync(o => o.UserId == id);
            if (hasOrders)
            {
                return BadRequest(new { message = "Нельзя удалить пользователя с заказами" });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Пользователь удалён" });
        }
    }
}