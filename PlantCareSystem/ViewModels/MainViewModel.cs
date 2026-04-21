using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PlantCareSystem.Views;
using System;
using System.Windows.Controls;

namespace PlantCareSystem.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;
        private UserControl? _currentView;

        public UserControl? CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public MainViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            NavigateCommand = new RelayCommand<string?>(Navigate);
            // По умолчанию показываем реестр растений
            Navigate("PlantRegistry");
        }

        public IRelayCommand<string?> NavigateCommand { get; }

        private void Navigate(string? viewName)
        {
            CurrentView = viewName switch
            {
                "PlantRegistry" => _serviceProvider.GetRequiredService<PlantRegistryView>(),
                "Calendar" => _serviceProvider.GetRequiredService<CareCalendarView>(),
                "Reports" => _serviceProvider.GetRequiredService<ReportView>(),
                _ => CurrentView
            };
        }
    }
}