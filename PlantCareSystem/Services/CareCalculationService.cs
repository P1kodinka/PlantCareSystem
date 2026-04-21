using Microsoft.Extensions.Configuration;
using PlantCareSystem.Models;
using System;
using System.Threading.Tasks;

namespace PlantCareSystem.Services
{
    public class CareCalculationService : ICareCalculationService
    {
        private readonly IConfiguration _configuration;

        public CareCalculationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<DateTime> CalculateNextDateAsync(int plantId, CareOperationType operationType, DateTime? lastDate = null)
        {
            // TODO: реализовать алгоритм с учётом сезонных коэффициентов
            await Task.Delay(10); // имитация работы
            var baseInterval = 7; // дней
            return DateTime.Now.AddDays(baseInterval);
        }
    }
}