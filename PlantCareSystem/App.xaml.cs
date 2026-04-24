using System;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantCareSystem.Data;
using PlantCareSystem.Services;
using PlantCareSystem.ViewModels;
using PlantCareSystem.Views;

namespace PlantCareSystem
{
    public partial class App : Application
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        public IConfiguration? Configuration { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            // Применяем миграции при запуске
            using (var scope = ServiceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.Migrate();
            }

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            var notificationService = ServiceProvider.GetRequiredService<INotificationService>();
            await notificationService.CheckAndNotifyAsync();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Получаем строку подключения и проверяем, что она не null
            var connectionString = Configuration?.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Строка подключения 'DefaultConnection' не найдена в конфигурации.");

            // DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString), ServiceLifetime.Transient);
            services.AddSingleton<IServiceScopeFactory>(provider =>
                provider.GetRequiredService<IServiceScopeFactory>());
            // Repositories (пока generic, позже уточним)
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Services (заглушки, реализуем позже)
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddScoped<ICareCalculationService, CareCalculationService>();
            services.AddScoped<IExportService, ExportService>();

            // ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<PlantListViewModel>();
            services.AddTransient<CalendarViewModel>();
            services.AddTransient<ReportViewModel>();

            // Views
            services.AddTransient<MainWindow>();
            services.AddTransient<PlantRegistryView>();
            services.AddTransient<CareCalendarView>();
            services.AddTransient<ReportView>();
            services.AddTransient<PlantEditWindow>();
            services.AddScoped<ICareCalculationService, CareCalculationService>();
            services.AddTransient<CalendarViewModel>();
            services.AddTransient<CareCalendarView>();
            services.AddScoped<IExportService, ExportService>();
            services.AddTransient<ReportViewModel>();
        }
    }
}