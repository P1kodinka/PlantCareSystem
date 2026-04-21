using System.Windows;
using PlantCareSystem.ViewModels;

namespace PlantCareSystem.Views
{
    public partial class MainWindow : Window
    {
        // Конструктор без параметров необходим для XAML-инициализации
        public MainWindow()
        {
            InitializeComponent();
        }

        // Конструктор с параметром используется DI-контейнером
        public MainWindow(MainViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}