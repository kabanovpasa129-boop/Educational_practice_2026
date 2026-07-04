namespace TechmartAPI.DTOs
{
    public class CreateOrderDto
    {
        public int UserId { get; set; }
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}