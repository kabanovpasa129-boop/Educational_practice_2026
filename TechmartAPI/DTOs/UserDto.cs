namespace TechmartAPI.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}