using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using PlantCareSystem.Data;
using PlantCareSystem.Models;
using PlantCareSystem.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PlantCareSystem.ViewModels
{
    public partial class ReportViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IExportService _exportService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private ObservableCollection<string> _reportTypes = new()
        {
            "График полива",
            "История ухода",
            "Редкие виды"
        };

        [ObservableProperty]
        private string _selectedReportType;

        [ObservableProperty]
        private DateTime _startDate = DateTime.Today.AddMonths(-1);

        [ObservableProperty]
        private DateTime _endDate = DateTime.Today;

        [ObservableProperty]
        private ObservableCollection<Plant> _plants = new();

        [ObservableProperty]
        private Plant? _selectedPlant;

        [ObservableProperty]
        private bool _isLoading;

        public ReportViewModel(
            IServiceProvider serviceProvider,
            IExportService exportService,
            INotificationService notificationService)
        {
            _serviceProvider = serviceProvider;
            _exportService = exportService;
            _notificationService = notificationService;

            SelectedReportType = ReportTypes.FirstOrDefault() ?? "";

            LoadPlantsCommand = new AsyncRelayCommand(LoadPlantsAsync);
            ExportCsvCommand = new AsyncRelayCommand(ExportCsvAsync);
            ExportPdfCommand = new AsyncRelayCommand(ExportPdfAsync);

            _ = LoadPlantsAsync();
        }

        public IAsyncRelayCommand LoadPlantsCommand { get; }
        public IAsyncRelayCommand ExportCsvCommand { get; }
        public IAsyncRelayCommand ExportPdfCommand { get; }

        private async Task LoadPlantsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var plants = await db.Plants.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();
            Plants = new ObservableCollection<Plant>(plants);
            SelectedPlant = Plants.FirstOrDefault();
        }

        private async Task<object?> GenerateReportDataAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            switch (SelectedReportType)
            {
                case "График полива":
                    var wateringOps = await db.CareOperations
                        .Include(o => o.Plant)
                        .Where(o => o.OperationType == CareOperationType.Watering &&
                                    o.OperationDate >= StartDate && o.OperationDate <= EndDate &&
                                    (SelectedPlant == null || o.PlantId == SelectedPlant.Id))
                        .OrderBy(o => o.OperationDate)
                        .Select(o => new
                        {
                            Дата = o.OperationDate.ToShortDateString(),
                            Растение = o.Plant.Name,
                            Статус = o.IsCompleted ? "Выполнено" : "Запланировано"
                        })
                        .ToListAsync();
                    return wateringOps;

                case "История ухода":
                    var allOps = await db.CareOperations
                        .Include(o => o.Plant)
                        .Where(o => o.OperationDate >= StartDate && o.OperationDate <= EndDate &&
                                    (SelectedPlant == null || o.PlantId == SelectedPlant.Id))
                        .OrderBy(o => o.OperationDate)
                        .Select(o => new
                        {
                            Дата = o.OperationDate.ToShortDateString(),
                            Тип = o.OperationType.ToString(),
                            Растение = o.Plant.Name,
                            Выполнено = o.IsCompleted ? "Да" : "Нет"
                        })
                        .ToListAsync();
                    return allOps;

                case "Редкие виды":
                    var rarePlants = await db.Plants
                        .Where(p => p.IsActive && p.IsRare)
                        .Select(p => new
                        {
                            Название = p.Name,
                            Семейство = p.Family ?? "-",
                            Вид = p.Species ?? "-",
                            Дата_посадки = p.PlantingDate.HasValue ? p.PlantingDate.Value.ToShortDateString() : "-",
                            Последний_полив = db.CareOperations
                                .Where(o => o.PlantId == p.Id && o.OperationType == CareOperationType.Watering)
                                .OrderByDescending(o => o.OperationDate)
                                .Select(o => o.OperationDate.ToShortDateString())
                                .FirstOrDefault() ?? "-"
                        })
                        .ToListAsync();
                    return rarePlants;

                default:
                    return null;
            }
        }

        private async Task ExportCsvAsync()
        {
            IsLoading = true;
            try
            {
                var data = await GenerateReportDataAsync();
                if (data == null)
                {
                    _notificationService.ShowNotification("Ошибка", "Нет данных для экспорта");
                    return;
                }

                var dialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    FileName = $"{SelectedReportType}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };
                if (dialog.ShowDialog() == true)
                {
                    var list = ((IEnumerable<dynamic>)data).Cast<object>();
                    await _exportService.ExportToCsvAsync(list, dialog.FileName);
                    _notificationService.ShowNotification("Готово", $"Отчёт сохранён в {dialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification("Ошибка экспорта CSV", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExportPdfAsync()
        {
            IsLoading = true;
            try
            {
                var data = await GenerateReportDataAsync();
                if (data == null)
                {
                    _notificationService.ShowNotification("Ошибка", "Нет данных для экспорта");
                    return;
                }

                var dialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = $"{SelectedReportType}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                };
                if (dialog.ShowDialog() == true)
                {
                    var list = ((IEnumerable<dynamic>)data).Cast<object>();
                    await _exportService.ExportToPdfAsync(SelectedReportType, list, dialog.FileName);
                    _notificationService.ShowNotification("Готово", $"Отчёт сохранён в {dialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification("Ошибка экспорта PDF", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}