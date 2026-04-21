using PlantCareSystem.Models;
using System;
using System.Threading.Tasks;

namespace PlantCareSystem.Services
{
    public interface ICareCalculationService
    {
        Task<DateTime> CalculateNextDateAsync(int plantId, CareOperationType operationType, DateTime? lastDate = null);
    }
}