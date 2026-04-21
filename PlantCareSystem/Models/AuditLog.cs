using System;
using System.ComponentModel.DataAnnotations;

namespace PlantCareSystem.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string EntityName { get; set; } = string.Empty;

        public int? EntityId { get; set; }

        public string? OldValues { get; set; }
        public string? NewValues { get; set; }

        [MaxLength(100)]
        public string? UserName { get; set; }
    }
}