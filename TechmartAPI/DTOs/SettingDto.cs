namespace TechmartAPI.DTOs
{
    public class SettingDto
    {
        public string Key { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateWeightsDto
    {
        public decimal CollaborativeWeight { get; set; }
        public decimal ContentBasedWeight { get; set; }
        public decimal AssociationWeight { get; set; }
    }
}