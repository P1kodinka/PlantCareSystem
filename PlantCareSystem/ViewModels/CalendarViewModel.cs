using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlantCareSystem.Data;
using PlantCareSystem.Models;
using PlantCareSystem.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PlantCareSystem.ViewModels
{
    public partial class CalendarViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly INotificationService _notificationService;
        private readonly ICareCalculationService _calculationService;
        private readonly SemaphoreSlim _loadSemaphore = new(1, 1);
        private bool _isLoaded;

        [ObservableProperty]
        private DateTime _currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        [ObservableProperty]
        private ObservableCollection<CalendarDay> _days = new();

        [ObservableProperty]
        private ObservableCollection<Plant> _allPlants = new();

        [ObservableProperty]
        private Plant? _selectedPlant;

        [ObservableProperty]
        private CareOperationType _selectedOperationType = CareOperationType.Watering;

        [ObservableProperty]
        private DateTime? _selectedDate;

        [ObservableProperty]
        private CalendarDay? _selectedDay;

        [ObservableProperty]
        private CareOperation? _selectedOperation;

        public ObservableCollection<CareOperationType> OperationTypes { get; } =
            new(Enum.GetValues<CareOperationType>());

        public CalendarViewModel(
            IServiceProvider serviceProvider,
            INotificationService notificationService,
            ICareCalculationService calculationService)
        {
            _serviceProvider = serviceProvider;
            _notificationService = notificationService;
            _calculationService = calculationService;

            LoadCommand = new AsyncRelayCommand(LoadMonthAsync);
            PreviousMonthCommand = new RelayCommand(PreviousMonth);
            NextMonthCommand = new RelayCommand(NextMonth);
            AddOperationCommand = new AsyncRelayCommand(AddOperationAsync);
            CompleteOperationCommand = new AsyncRelayCommand<CareOperation>(CompleteOperationAsync);
            DeleteOperationCommand = new AsyncRelayCommand<CareOperation>(DeleteOperationAsync);
            GenerateScheduleCommand = new AsyncRelayCommand(GenerateScheduleForPlantAsync);
        }

        public IAsyncRelayCommand LoadCommand { get; }
        public IRelayCommand PreviousMonthCommand { get; }
        public IRelayCommand NextMonthCommand { get; }
        public IAsyncRelayCommand AddOperationCommand { get; }
        public IAsyncRelayCommand<CareOperation> CompleteOperationCommand { get; }
        public IAsyncRelayCommand<CareOperation> DeleteOperationCommand { get; }
        public IAsyncRelayCommand GenerateScheduleCommand { get; }

        public async Task InitializeAsync()
        {
            if (!_isLoaded)
            {
                await LoadMonthAsync();
                _isLoaded = true;
            }
        }

        private async Task LoadMonthAsync()
        {
            await _loadSemaphore.WaitAsync();
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Загружаем все активные растения для выпадающего списка (один раз)
                if (AllPlants.Count == 0)
                {
                    var plants = await dbContext.Plants
                        .Where(p => p.IsActive)
                        .OrderBy(p => p.Name)
                        .ToListAsync();
                    AllPlants = new ObservableCollection<Plant>(plants);
                    SelectedPlant = AllPlants.FirstOrDefault();
                }

                await RefreshCalendarDaysAsync(dbContext);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification("Ошибка загрузки календаря", ex.Message);
            }
            finally
            {
                _loadSemaphore.Release();
            }
        }

        private async Task RefreshCalendarDaysAsync(AppDbContext dbContext)
        {
            var start = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
            var end = start.AddMonths(1).AddDays(-1);
            var daysInMonth = DateTime.DaysInMonth(CurrentMonth.Year, CurrentMonth.Month);

            var operations = await dbContext.CareOperations
                .Include(o => o.Plant)
                .Where(o => o.OperationDate >= start.AddDays(-7) && o.OperationDate <= end.AddDays(7))
                .ToListAsync();

            var newDays = new ObservableCollection<CalendarDay>();
            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(CurrentMonth.Year, CurrentMonth.Month, day);
                var dayOps = operations.Where(o => o.OperationDate.Date == date).ToList();
                newDays.Add(new CalendarDay
                {
                    Date = date,
                    Operations = new ObservableCollection<CareOperation>(dayOps)
                });
            }
            Days = newDays;
        }

        private void PreviousMonth() => CurrentMonth = CurrentMonth.AddMonths(-1);
        private void NextMonth() => CurrentMonth = CurrentMonth.AddMonths(1);

        partial void OnCurrentMonthChanged(DateTime value)
        {
            _ = LoadMonthAsync();
        }

        private async Task AddOperationAsync()
        {
            if (SelectedPlant == null)
            {
                _notificationService.ShowNotification("Ошибка", "Выберите растение");
                return;
            }
            if (SelectedDate == null)
            {
                _notificationService.ShowNotification("Ошибка", "Выберите дату");
                return;
            }

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var operation = new CareOperation
                {
                    PlantId = SelectedPlant.Id,
                    OperationType = SelectedOperationType,
                    OperationDate = SelectedDate.Value,
                    PlannedDate = SelectedDate.Value,
                    IsCompleted = false,
                    PerformedBy = Environment.UserName
                };

                await dbContext.CareOperations.AddAsync(operation);
                await dbContext.SaveChangesAsync();

                // Перезагружаем календарь (создаём новый scope)
                await LoadMonthAsync();
                _notificationService.ShowNotification("Успех", "Операция добавлена");
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification("Ошибка", ex.Message);
            }
        }

        private async Task CompleteOperationAsync(CareOperation? operation)
        {
            if (operation == null) return;

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var trackedOp = await dbContext.CareOperations.FindAsync(operation.Id);
                if (trackedOp == null) return;

                trackedOp.IsCompleted = true;
                await dbContext.SaveChangesAsync();

                await _calculationService.CalculateNextDateAsync(trackedOp.PlantId, trackedOp.OperationType, trackedOp.OperationDate);

                await LoadMonthAsync();
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification("Ошибка", ex.Message);
            }
        }

        private async Task DeleteOperationAsync(CareOperation? operation)
        {
            if (operation == null) return;
            if (MessageBox.Show("Удалить операцию?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var trackedOp = await dbContext.CareOperations.FindAsync(operation.Id);
                if (trackedOp != null)
                {
                    dbContext.CareOperations.Remove(trackedOp);
                    await dbContext.SaveChangesAsync();
                }

                await LoadMonthAsync();
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification("Ошибка", ex.Message);
            }
        }

        private async Task GenerateScheduleForPlantAsync()
        {
            if (SelectedPlant == null) return;

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var schedules = await dbContext.CareSchedules
                    .Where(s => s.PlantId == SelectedPlant.Id && s.IsActive)
                    .ToListAsync();

                foreach (var sch in schedules)
                {
                    var nextDate = await _calculationService.CalculateNextDateAsync(sch.PlantId, sch.OperationType);
                    var operation = new CareOperation
                    {
                        PlantId = sch.PlantId,
                        OperationType = sch.OperationType,
                        OperationDate = nextDate,
                        PlannedDate = nextDate,
                        IsCompleted = false,
                        Notes = "Автоматически запланировано"
                    };
                    await dbContext.CareOperations.AddAsync(operation);
                }
                await dbContext.SaveChangesAsync();
                await LoadMonthAsync();
                _notificationService.ShowNotification("Успех", "График сгенерирован");
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification("Ошибка", ex.Message);
            }
        }

        [RelayCommand]
        private void SelectDay(CalendarDay? day)
        {
            SelectedDay = day;
            SelectedDate = day?.Date;
        }

        [RelayCommand]
        private async Task CompleteSelectedOperationAsync()
        {
            if (SelectedOperation != null)
                await CompleteOperationAsync(SelectedOperation);
        }

        [RelayCommand]
        private async Task DeleteSelectedOperationAsync()
        {
            if (SelectedOperation != null)
                await DeleteOperationAsync(SelectedOperation);
        }
    }

    public partial class CalendarDay : ObservableObject
    {
        public DateTime Date { get; set; }
        [ObservableProperty]
        private ObservableCollection<CareOperation> _operations = new();
    }
}