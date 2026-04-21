using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlantCareSystem.Data;
using PlantCareSystem.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PlantCareSystem.ViewModels
{
    public partial class PlantEditViewModel : ObservableValidator
    {
        private readonly AppDbContext _dbContext;
        private readonly Window _window;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string? _family;

        [ObservableProperty]
        private string? _genus;

        [ObservableProperty]
        private string? _species;

        [ObservableProperty]
        private string? _variety;

        [ObservableProperty]
        private string? _location;

        [ObservableProperty]
        private DateTime? _plantingDate;

        [ObservableProperty]
        private string? _notes;

        [ObservableProperty]
        private bool _isActive = true;

        private int? _plantId;

        public PlantEditViewModel(Plant? plant, AppDbContext dbContext, Window window)
        {
            _dbContext = dbContext;
            _window = window;

            if (plant != null)
            {
                _plantId = plant.Id;
                Name = plant.Name;
                Family = plant.Family;
                Genus = plant.Genus;
                Species = plant.Species;
                Variety = plant.Variety;
                Location = plant.Location;
                PlantingDate = plant.PlantingDate;
                Notes = plant.Notes;
                IsActive = plant.IsActive;
            }

            SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);

            // Первичная валидация
            ValidateAllProperties();
            SaveCommand.NotifyCanExecuteChanged();
        }

        public IAsyncRelayCommand SaveCommand { get; }

        partial void OnNameChanged(string value)
        {
            ValidateSingleProperty(value, nameof(Name));
            SaveCommand.NotifyCanExecuteChanged();
        }

        private bool CanSave() => !HasErrors;

        private async Task SaveAsync()
        {
            ValidateAllProperties();
            if (HasErrors)
            {
                var firstError = GetErrors().FirstOrDefault()?.ErrorMessage ?? "Некорректные данные";
                MessageBox.Show(firstError, "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Plant plant;
                if (_plantId.HasValue)
                {
                    plant = await _dbContext.Plants.FindAsync(_plantId.Value);
                    if (plant == null) throw new Exception("Растение не найдено");
                }
                else
                {
                    plant = new Plant();
                    await _dbContext.Plants.AddAsync(plant);
                }

                plant.Name = Name;
                plant.Family = Family;
                plant.Genus = Genus;
                plant.Species = Species;
                plant.Variety = Variety;
                plant.Location = Location;
                plant.PlantingDate = PlantingDate;
                plant.Notes = Notes;
                plant.IsActive = IsActive;

                await _dbContext.SaveChangesAsync();
                _window.DialogResult = true;
                _window.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ValidateSingleProperty(object? value, string propertyName)
        {
            var context = new ValidationContext(this) { MemberName = propertyName };
            var results = new List<ValidationResult>();
            Validator.TryValidateProperty(value, context, results);

            ClearErrors(propertyName);
            foreach (var result in results)
                AddError(result.ErrorMessage ?? "Ошибка", propertyName);
        }
    }
}