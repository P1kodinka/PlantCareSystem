using System.Windows.Controls;
using PlantCareSystem.ViewModels;

namespace PlantCareSystem.Views
{
    public partial class CareCalendarView : UserControl
    {
        public CareCalendarView(CalendarViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}