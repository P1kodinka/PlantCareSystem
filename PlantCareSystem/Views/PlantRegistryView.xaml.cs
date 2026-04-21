using System.Windows.Controls;
using PlantCareSystem.ViewModels;

namespace PlantCareSystem.Views
{
    public partial class PlantRegistryView : UserControl
    {
        public PlantRegistryView(PlantListViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}