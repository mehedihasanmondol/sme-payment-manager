using System.Windows.Controls;
using BillPaymentManager.ViewModels;

namespace BillPaymentManager.Views;

/// <summary>
/// Interaction logic for DashboardView.xaml
/// </summary>
public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        this.Loaded += DashboardView_Loaded;
    }

    private void DashboardView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is DashboardViewModel viewModel)
        {
            viewModel.LoadDataCommand.Execute(null);
        }
    }
}
