using System.Windows.Controls;
using PlantCareSystem.ViewModels;

namespace PlantCareSystem.Views
{
    public partial class PlantRegistryView : UserControl
    {
        public PlantRegistryView()
        {
            InitializeComponent();
        }

        public PlantRegistryView(PlantListViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}