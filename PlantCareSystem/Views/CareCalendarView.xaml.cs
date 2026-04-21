using System.Windows.Controls;
using PlantCareSystem.ViewModels;

namespace PlantCareSystem.Views
{
    public partial class CareCalendarView : UserControl
    {
        public CareCalendarView()
        {
            InitializeComponent();
        }

        public CareCalendarView(CalendarViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}