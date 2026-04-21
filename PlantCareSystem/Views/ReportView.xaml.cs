using System.Windows.Controls;
using PlantCareSystem.ViewModels;

namespace PlantCareSystem.Views
{
    public partial class ReportView : UserControl
    {
        public ReportView()
        {
            InitializeComponent();
        }

        public ReportView(ReportViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}