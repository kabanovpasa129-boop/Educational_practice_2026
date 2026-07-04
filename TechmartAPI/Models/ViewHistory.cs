using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechmartAPI.Models
{
    [Table("Просмотры")]
    public class ViewHistory
    {
        [Key]
        [Column("Идентификатор")]
        public int Id { get; set; }

        [Column("ИдентификаторПользователя")]
        public int UserId { get; set; }

        [Column("ИдентификаторТовара")]
        public int ProductId { get; set; }

        [Column("ДатаПросмотра")]
        public DateTime ViewDate { get; set; }
    }
}