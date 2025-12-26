using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BillPaymentManager.ViewModels.Base;

namespace BillPaymentManager.ViewModels;

/// <summary>
/// Main window ViewModel for navigation
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase? _currentViewModel;

    [ObservableProperty]
    private string _title = "Bill Payment Manager";

    public DashboardViewModel DashboardViewModel { get; }
    public AddPaymentViewModel AddPaymentViewModel { get; }
    public HistoryViewModel HistoryViewModel { get; }

    public SettingsViewModel SettingsViewModel { get; }
    public DeveloperInfoViewModel DeveloperInfoViewModel { get; }

    public MainViewModel()
    {
        DashboardViewModel = new DashboardViewModel();
        AddPaymentViewModel = new AddPaymentViewModel();
        HistoryViewModel = new HistoryViewModel();

        SettingsViewModel = new SettingsViewModel();
        DeveloperInfoViewModel = new DeveloperInfoViewModel();

        // Set dashboard as default view
        CurrentViewModel = DashboardViewModel;
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        CurrentViewModel = DashboardViewModel;
        DashboardViewModel.LoadDataCommand.Execute(null);
    }

    [RelayCommand]
    private void NavigateToAddPayment()
    {
        CurrentViewModel = AddPaymentViewModel;
    }

    [RelayCommand]
    private void NavigateToHistory()
    {
        CurrentViewModel = HistoryViewModel;
        HistoryViewModel.LoadDataCommand.Execute(null);
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        CurrentViewModel = SettingsViewModel;
    }


    [RelayCommand]
    private void NavigateToDeveloperInfo()
    {
        CurrentViewModel = DeveloperInfoViewModel;
    }
}
