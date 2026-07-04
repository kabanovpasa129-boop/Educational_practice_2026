using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechmartAPI.Models
{
    [Table("Заказы")]
    public class Order
    {
        [Key]
        [Column("Идентификатор")]
        public int Id { get; set; }

        [Column("ИдентификаторПользователя")]
        public int UserId { get; set; }

        [Column("ДатаЗаказа")]
        public DateTime OrderDate { get; set; }

        [Column("Статус")]
        public string Status { get; set; } = string.Empty;
    }
}