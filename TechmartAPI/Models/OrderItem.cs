using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechmartAPI.Models
{
    [Table("Позиции Заказов")]
    public class OrderItem
    {
        [Key]
        [Column("Идентификатор")]
        public int Id { get; set; }

        [Column("ИдентификаторЗаказа")]
        public int OrderId { get; set; }

        [Column("ИдентификаторТовара")]
        public int ProductId { get; set; }

        [Column("Количество")]
        public int Quantity { get; set; }
    }
}