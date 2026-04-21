using System.Windows;
using PlantCareSystem.Data;
using PlantCareSystem.Models;
using PlantCareSystem.ViewModels;

namespace PlantCareSystem.Views
{
    public partial class PlantEditWindow : Window
    {
        public PlantEditWindow(Plant? plant, AppDbContext dbContext)
        {
            InitializeComponent();
            DataContext = new PlantEditViewModel(plant, dbContext, this);
        }
    }
}