using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using PlantCareSystem.Data;

namespace PlantCareSystem.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _dbContext;

        public NotificationService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void ShowNotification(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public async Task CheckAndNotifyAsync()
        {
            var today = DateTime.Today;
            var upcomingDays = 3; // предупреждать за 3 дня

            var upcomingOps = await _dbContext.CareOperations
                .Include(o => o.Plant)
                .Where(o => !o.IsCompleted && o.PlannedDate.HasValue &&
                            o.PlannedDate.Value.Date >= today &&
                            o.PlannedDate.Value.Date <= today.AddDays(upcomingDays))
                .ToListAsync();

            if (upcomingOps.Any())
            {
                var message = string.Join("\n", upcomingOps.Select(o =>
                    $"{o.OperationType} для {o.Plant.Name} ({o.PlannedDate.Value:dd.MM.yyyy})"));
                ShowNotification("Напоминание о предстоящих работах", message);
            }

            var overdueOps = await _dbContext.CareOperations
                .Include(o => o.Plant)
                .Where(o => !o.IsCompleted && o.PlannedDate.HasValue &&
                            o.PlannedDate.Value.Date < today)
                .ToListAsync();

            if (overdueOps.Any())
            {
                var message = string.Join("\n", overdueOps.Select(o =>
                    $"{o.OperationType} для {o.Plant.Name} (просрочено с {o.PlannedDate.Value:dd.MM.yyyy})"));
                ShowNotification("Просроченные операции", message);
            }
        }
    }
}