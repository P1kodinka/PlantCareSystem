using System.Windows.Controls;
using PlantCareSystem.ViewModels;

namespace PlantCareSystem.Views
{
    public partial class ReportView : UserControl
    {
        public ReportView(ReportViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}