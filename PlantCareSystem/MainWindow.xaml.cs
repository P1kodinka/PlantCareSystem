using System.Windows;
using PlantCareSystem.ViewModels;

namespace PlantCareSystem.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}