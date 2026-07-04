using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechmartAPI.Models
{
    [Table("Пользователи")]
    public class User
    {
        [Key]
        [Column("Идентификатор")]
        public int Id { get; set; }

        [Column("Логин")]
        public string Login { get; set; } = string.Empty;

        [Column("Пароль")]
        public string Password { get; set; } = string.Empty;

        [Column("Роль")]
        public string Role { get; set; } = string.Empty;
    }
}