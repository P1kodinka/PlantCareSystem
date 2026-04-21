using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlantCareSystem.Models
{
    public class CareSchedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PlantId { get; set; }

        [ForeignKey(nameof(PlantId))]
        public virtual Plant Plant { get; set; } = null!;

        public CareOperationType OperationType { get; set; }

        public int BaseIntervalDays { get; set; }

        public double SeasonalCoefficient { get; set; } = 1.0;

        public DateTime? LastPerformedDate { get; set; }

        public DateTime? NextPlannedDate { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(200)]
        public string? Notes { get; set; }
    }
}