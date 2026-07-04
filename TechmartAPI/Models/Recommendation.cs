using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechmartAPI.Models
{
    [Table("Рекомендации")]
    public class Recommendation
    {
        [Key]
        [Column("Идентификатор")]
        public int Id { get; set; }

        [Column("ИдентификаторПользователя")]
        public int UserId { get; set; }

        [Column("ИдентификаторТовара")]
        public int ProductId { get; set; }

        [Column("ДатаГенерации")]
        public DateTime GenerationDate { get; set; }

        [Column("Рейтинг")]
        public decimal Rating { get; set; }
    }
}