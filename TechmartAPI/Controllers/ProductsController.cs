using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechmartAPI.Data;
using TechmartAPI.DTOs;

namespace TechmartAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
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

        [HttpGet("popular")]
        public async Task<IActionResult> GetPopularProducts()
        {
            var popular = await _context.OrderItems
                .GroupBy(oi => oi.ProductId)
                .Select(g => new { ProductId = g.Key, Count = g.Sum(oi => oi.Quantity) })
                .OrderByDescending(x => x.Count)
                .Take(8)
                .Join(_context.Products, x => x.ProductId, p => p.Id, (x, p) => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Price = p.Price
                })
                .ToListAsync();

            if (!popular.Any())
            {
                var fallback = await _context.Products.Take(8).Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Price = p.Price
                }).ToListAsync();
                return Ok(fallback);
            }

            return Ok(popular);
        }

        [HttpGet("recommendations/{userId}")]
        public async Task<IActionResult> GetRecommendations(int userId)
        {
            var recommendations = await _context.Recommendations
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Rating)
                .Take(8)
                .Join(_context.Products, r => r.ProductId, p => p.Id, (r, p) => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Price = p.Price
                })
                .ToListAsync();

            return Ok(recommendations);
        }

        [HttpGet("together/{productId}")]
        public async Task<IActionResult> GetTogetherProducts(int productId)
        {
            var ordersWithProduct = await _context.OrderItems
                .Where(oi => oi.ProductId == productId)
                .Select(oi => oi.OrderId)
                .Distinct()
                .ToListAsync();

            var together = await _context.OrderItems
                .Where(oi => ordersWithProduct.Contains(oi.OrderId) && oi.ProductId != productId)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new { ProductId = g.Key, Count = g.Sum(oi => oi.Quantity) })
                .OrderByDescending(x => x.Count)
                .Take(8)
                .Join(_context.Products, x => x.ProductId, p => p.Id, (x, p) => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Price = p.Price
                })
                .ToListAsync();

            if (!together.Any())
            {
                var fallback = await _context.Products.Take(8).Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Price = p.Price
                }).ToListAsync();
                return Ok(fallback);
            }

            return Ok(together);
        }
    }
}