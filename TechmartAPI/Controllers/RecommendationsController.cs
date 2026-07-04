using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechmartAPI.Data;
using TechmartAPI.DTOs;

namespace TechmartAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RecommendationsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Recommendations/popular
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

        // GET: api/Recommendations/together/{productId}
        [HttpGet("together/{productId}")]
        public async Task<IActionResult> GetTogetherProducts(int productId)
        {
            var ordersWithProduct = await _context.OrderItems
                .Where(oi => oi.ProductId == productId)
                .Select(oi => oi.OrderId)
                .Distinct()
                .ToListAsync();

            if (!ordersWithProduct.Any())
            {
                // Возвращаем популярные товары через Ok()
                var fallback = await _context.Products.Take(8).Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Price = p.Price
                }).ToListAsync();
                return Ok(fallback);
            }

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

        // GET: api/Recommendations/hybrid/{userId}
        [HttpGet("hybrid/{userId}")]
        public async Task<IActionResult> GetHybridRecommendations(int userId)
        {
            var settings = await _context.Settings.ToListAsync();
            var collaborativeWeight = settings.FirstOrDefault(s => s.Key == "ВесКоллаборативнойФильтрации")?.Value ?? 0.40m;
            var contentBasedWeight = settings.FirstOrDefault(s => s.Key == "ВесКонтентнойФильтрации")?.Value ?? 0.35m;
            var associationWeight = settings.FirstOrDefault(s => s.Key == "ВесПравилАссоциации")?.Value ?? 0.25m;

            int collaborativeCount = (int)(8 * collaborativeWeight);
            int contentBasedCount = (int)(8 * contentBasedWeight);
            int associationCount = 8 - collaborativeCount - contentBasedCount;

            var collaborativeRecs = await GetCollaborativeRecommendations(userId);
            var contentBasedRecs = await GetContentBasedRecommendations(userId);
            var associationRecs = await GetAssociationRecommendations(userId);

            var allRecs = new List<ProductDto>();
            allRecs.AddRange(collaborativeRecs.Take(collaborativeCount));
            allRecs.AddRange(contentBasedRecs.Take(contentBasedCount));
            allRecs.AddRange(associationRecs.Take(associationCount));

            var result = allRecs.DistinctBy(p => p.Id).Take(8).ToList();
            return Ok(result);
        }

        private async Task<List<ProductDto>> GetCollaborativeRecommendations(int userId)
        {
            var purchasedCategories = await _context.Orders
                .Where(o => o.UserId == userId)
                .Join(_context.OrderItems, o => o.Id, oi => oi.OrderId, (o, oi) => oi.ProductId)
                .Join(_context.Products, pid => pid, p => p.Id, (pid, p) => p.Category)
                .Distinct()
                .ToListAsync();

            if (!purchasedCategories.Any())
            {
                return await _context.Products.Take(6).Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Price = p.Price
                }).ToListAsync();
            }

            return await _context.Products
                .Where(p => purchasedCategories.Contains(p.Category))
                .Take(6)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Price = p.Price
                })
                .ToListAsync();
        }

        private async Task<List<ProductDto>> GetContentBasedRecommendations(int userId)
        {
            var viewedCategories = await _context.ViewHistories
                .Where(vh => vh.UserId == userId)
                .Join(_context.Products, vh => vh.ProductId, p => p.Id, (vh, p) => p.Category)
                .Distinct()
                .ToListAsync();

            if (!viewedCategories.Any())
            {
                return await _context.Products.OrderBy(p => p.Id).Take(6).Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Price = p.Price
                }).ToListAsync();
            }

            return await _context.Products
                .Where(p => viewedCategories.Contains(p.Category))
                .Take(6)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Price = p.Price
                })
                .ToListAsync();
        }

        private async Task<List<ProductDto>> GetAssociationRecommendations(int userId)
        {
            var userOrders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Select(o => o.Id)
                .ToListAsync();

            if (!userOrders.Any())
            {
                return await _context.Products.Take(6).Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Price = p.Price
                }).ToListAsync();
            }

            var userProducts = await _context.OrderItems
                .Where(oi => userOrders.Contains(oi.OrderId))
                .Select(oi => oi.ProductId)
                .Distinct()
                .ToListAsync();

            var ordersWithUserProducts = await _context.OrderItems
                .Where(oi => userProducts.Contains(oi.ProductId))
                .Select(oi => oi.OrderId)
                .Distinct()
                .ToListAsync();

            var recommendations = await _context.OrderItems
                .Where(oi => ordersWithUserProducts.Contains(oi.OrderId) && !userProducts.Contains(oi.ProductId))
                .GroupBy(oi => oi.ProductId)
                .Select(g => new { ProductId = g.Key, Count = g.Sum(oi => oi.Quantity) })
                .OrderByDescending(x => x.Count)
                .Take(6)
                .Join(_context.Products, x => x.ProductId, p => p.Id, (x, p) => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Price = p.Price
                })
                .ToListAsync();

            if (!recommendations.Any())
            {
                return await _context.Products.Take(6).Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Price = p.Price
                }).ToListAsync();
            }

            return recommendations;
        }
    }
}