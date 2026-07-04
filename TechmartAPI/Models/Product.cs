using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechmartAPI.Models
{
    [Table("Товары")]
    public class Product
    {
        [Key]
        [Column("Идентификатор")]
        public int Id { get; set; }

        [Column("Название")]
        public string Name { get; set; } = string.Empty;

        [Column("Категория")]
        public string Category { get; set; } = string.Empty;

        [Column("Цена")]
        public decimal Price { get; set; }
    }
}