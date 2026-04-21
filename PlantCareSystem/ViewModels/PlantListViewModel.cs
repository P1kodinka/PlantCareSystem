using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PlantCareSystem.Data;
using PlantCareSystem.Models;
using PlantCareSystem.Services;
using PlantCareSystem.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PlantCareSystem.ViewModels
{
    public partial class PlantListViewModel : ObservableObject
    {
        private readonly AppDbContext _dbContext;
        private readonly INotificationService _notificationService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private ObservableCollection<Plant> _plants = new();

        [ObservableProperty]
        private Plant? _selectedPlant;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _showInactive = false;

        private ICollectionView _plantsView;

        public ICollectionView PlantsView
        {
            get => _plantsView;
            private set => SetProperty(ref _plantsView, value);
        }

        public PlantListViewModel(AppDbContext dbContext, INotificationService notificationService, IServiceProvider serviceProvider)
        {
            _dbContext = dbContext;
            _notificationService = notificationService;
            _serviceProvider = serviceProvider;

            // Команды
            LoadCommand = new AsyncRelayCommand(LoadPlantsAsync);
            AddCommand = new AsyncRelayCommand(AddPlantAsync);
            EditCommand = new AsyncRelayCommand(EditPlantAsync, () => SelectedPlant != null);
            DeleteCommand = new AsyncRelayCommand(DeletePlantAsync, () => SelectedPlant != null);
            ToggleActiveCommand = new AsyncRelayCommand(ToggleActiveAsync, () => SelectedPlant != null);
            RefreshCommand = new AsyncRelayCommand(LoadPlantsAsync);
            ApplyFilterCommand = new RelayCommand(ApplyFilter);

            // Загружаем данные при создании
            LoadCommand.Execute(null);
        }

        public IAsyncRelayCommand LoadCommand { get; }
        public IAsyncRelayCommand AddCommand { get; }
        public IAsyncRelayCommand EditCommand { get; }
        public IAsyncRelayCommand DeleteCommand { get; }
        public IAsyncRelayCommand ToggleActiveCommand { get; }
        public IAsyncRelayCommand RefreshCommand { get; }
        public IRelayCommand ApplyFilterCommand { get; }

        private async Task LoadPlantsAsync()
        {
            try
            {
                var query = _dbContext.Plants.AsNoTracking();
                if (!ShowInactive)
                    query = query.Where(p => p.IsActive);

                var plantsFromDb = await query.OrderBy(p => p.Name).ToListAsync();
                Plants = new ObservableCollection<Plant>(plantsFromDb);

                PlantsView = CollectionViewSource.GetDefaultView(Plants);
                PlantsView.Filter = FilterPlants;
                ApplyFilter();
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification("Ошибка загрузки", $"Не удалось загрузить список растений: {ex.Message}");
            }
        }

        private bool FilterPlants(object obj)
        {
            if (obj is not Plant plant) return false;
            if (string.IsNullOrWhiteSpace(SearchText)) return true;

            var search = SearchText.Trim();
            return (plant.Name?.Contains(search, StringComparison.OrdinalIgnoreCase) == true) ||
                   (plant.Family?.Contains(search, StringComparison.OrdinalIgnoreCase) == true) ||
                   (plant.Genus?.Contains(search, StringComparison.OrdinalIgnoreCase) == true) ||
                   (plant.Species?.Contains(search, StringComparison.OrdinalIgnoreCase) == true) ||
                   (plant.Variety?.Contains(search, StringComparison.OrdinalIgnoreCase) == true);
        }

        private void ApplyFilter()
        {
            PlantsView?.Refresh();
        }

        partial void OnSearchTextChanged(string value) => ApplyFilter();
        partial void OnShowInactiveChanged(bool value) => LoadCommand.Execute(null);

        private async Task AddPlantAsync()
        {
            var dialog = new PlantEditWindow(null, _dbContext);
            dialog.Owner = Application.Current.MainWindow;
            if (dialog.ShowDialog() == true)
            {
                await LoadPlantsAsync();
                _notificationService.ShowNotification("Успех", "Растение добавлено.");
            }
        }

        private async Task EditPlantAsync()
        {
            if (SelectedPlant == null) return;
            // Загружаем полную сущность для редактирования (т.к. AsNoTracking)
            var plantToEdit = await _dbContext.Plants.FindAsync(SelectedPlant.Id);
            if (plantToEdit == null) return;

            var dialog = new PlantEditWindow(plantToEdit, _dbContext);
            dialog.Owner = Application.Current.MainWindow;
            if (dialog.ShowDialog() == true)
            {
                await LoadPlantsAsync();
                _notificationService.ShowNotification("Успех", "Данные растения обновлены.");
            }
        }

        private async Task DeletePlantAsync()
        {
            if (SelectedPlant == null) return;
            var result = MessageBox.Show($"Удалить растение '{SelectedPlant.Name}'? Это действие необратимо.",
                                          "Подтверждение удаления",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                var plant = await _dbContext.Plants.FindAsync(SelectedPlant.Id);
                if (plant != null)
                {
                    _dbContext.Plants.Remove(plant);
                    await _dbContext.SaveChangesAsync();
                    await LoadPlantsAsync();
                    _notificationService.ShowNotification("Успех", "Растение удалено.");
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification("Ошибка удаления", ex.Message);
            }
        }

        private async Task ToggleActiveAsync()
        {
            if (SelectedPlant == null) return;
            try
            {
                var plant = await _dbContext.Plants.FindAsync(SelectedPlant.Id);
                if (plant != null)
                {
                    plant.IsActive = !plant.IsActive;
                    await _dbContext.SaveChangesAsync();
                    await LoadPlantsAsync();
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification("Ошибка", ex.Message);
            }
        }
    }
}