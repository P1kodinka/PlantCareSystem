using Microsoft.EntityFrameworkCore;
using PlantCareSystem.Data;
using PlantCareSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PlantCareSystem.Services
{
    public class CareCalculationService : ICareCalculationService
    {
        private readonly AppDbContext _dbContext;

        public CareCalculationService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DateTime> CalculateNextDateAsync(int plantId, CareOperationType operationType, DateTime? lastDate = null)
        {
            // Получаем норматив для данного растения и типа операции
            var schedule = await _dbContext.CareSchedules
                .FirstOrDefaultAsync(s => s.PlantId == plantId && s.OperationType == operationType && s.IsActive);

            int baseInterval = schedule?.BaseIntervalDays ?? 7;
            double coefficient = schedule?.SeasonalCoefficient ?? 1.0;

            // Упрощённая сезонная логика: если зима, увеличиваем интервал
            var today = DateTime.Today;
            if (today.Month == 12 || today.Month == 1 || today.Month == 2)
                coefficient *= 1.5; // зимой реже полив

            DateTime referenceDate = lastDate ?? DateTime.Today;
            int interval = (int)(baseInterval * coefficient);
            DateTime nextDate = referenceDate.AddDays(interval);

            // Обновляем расписание
            if (schedule != null)
            {
                schedule.LastPerformedDate = referenceDate;
                schedule.NextPlannedDate = nextDate;
                _dbContext.CareSchedules.Update(schedule);
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                // Создаём новое расписание, если не было
                schedule = new CareSchedule
                {
                    PlantId = plantId,
                    OperationType = operationType,
                    BaseIntervalDays = baseInterval,
                    SeasonalCoefficient = coefficient,
                    LastPerformedDate = referenceDate,
                    NextPlannedDate = nextDate,
                    IsActive = true
                };
                await _dbContext.CareSchedules.AddAsync(schedule);
                await _dbContext.SaveChangesAsync();
            }

            return nextDate;
        }
    }
}