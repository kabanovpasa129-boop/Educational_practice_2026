using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechmartAPI.Models
{
    [Table("Настройки")]
    public class Setting
    {
        [Key]
        [Column("Идентификатор")]
        public int Id { get; set; }

        [Column("Ключ")]
        public string Key { get; set; } = string.Empty;

        [Column("Значение")]
        public decimal Value { get; set; }

        [Column("Описание")]
        public string? Description { get; set; }
    }
}