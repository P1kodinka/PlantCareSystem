using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlantCareSystem.Models
{
    public class CareOperation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PlantId { get; set; }

        [ForeignKey(nameof(PlantId))]
        public virtual Plant Plant { get; set; } = null!;

        public CareOperationType OperationType { get; set; }

        public DateTime OperationDate { get; set; }

        public DateTime? PlannedDate { get; set; }

        public bool IsCompleted { get; set; } = true;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? PerformedBy { get; set; }
    }
}