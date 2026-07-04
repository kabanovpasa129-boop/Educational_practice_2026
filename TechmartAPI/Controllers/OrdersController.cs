using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechmartAPI.Data;
using TechmartAPI.DTOs;
using TechmartAPI.Models;

namespace TechmartAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

      
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto orderDto)
        {
            if (orderDto == null || orderDto.UserId <= 0 || orderDto.Items == null || orderDto.Items.Count == 0)
            {
                return BadRequest(new { message = "Некорректные данные заказа" });
            }

            var user = await _context.Users.FindAsync(orderDto.UserId);
            if (user == null)
            {
                return BadRequest(new { message = "Пользователь не найден" });
            }

            var order = new Order
            {
                UserId = orderDto.UserId,
                OrderDate = DateTime.Now,
                Status = "новый"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in orderDto.Items)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                };
                _context.OrderItems.Add(orderItem);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Заказ успешно оформлен", orderId = order.Id });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserOrders(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "Пользователь не найден" });
            }

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    o.Id,
                    o.OrderDate,
                    o.Status,
                    Items = _context.OrderItems
                        .Where(oi => oi.OrderId == o.Id)
                        .Join(_context.Products, oi => oi.ProductId, p => p.Id, (oi, p) => new
                        {
                            p.Name,
                            p.Category,
                            p.Price,
                            oi.Quantity,
                            Total = p.Price * oi.Quantity
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }
    }
}