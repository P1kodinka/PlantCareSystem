using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlantCareSystem.Models
{
    public class Plant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Family { get; set; }

        [MaxLength(50)]
        public string? Genus { get; set; }

        [MaxLength(50)]
        public string? Species { get; set; }

        [MaxLength(50)]
        public string? Variety { get; set; }

        public DateTime? PlantingDate { get; set; }
        public DateTime? LastTransplantDate { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsRare { get; set; } = false;   // <-- новое поле

        // Навигационные свойства
        public virtual ICollection<CareOperation> CareOperations { get; set; } = new List<CareOperation>();
        public virtual ICollection<CareSchedule> CareSchedules { get; set; } = new List<CareSchedule>();
    }
}