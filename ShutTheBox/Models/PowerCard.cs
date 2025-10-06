using ShutTheTwelve.Backend.Enums;
using System.ComponentModel.DataAnnotations;

namespace ShutTheTwelve.Backend.Models
{
    public class PowerCard
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        public int Cost { get; set; }

        [Required]
        [MaxLength(50)]
        public string EffectType { get; set; } = string.Empty; // LockAndReroll, Sabotage, etc.

        public bool IsActive { get; set; } = true;
    }
}