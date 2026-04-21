using System.Windows;

namespace PlantCareSystem.Services
{
    public class NotificationService : INotificationService
    {
        public void ShowNotification(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}