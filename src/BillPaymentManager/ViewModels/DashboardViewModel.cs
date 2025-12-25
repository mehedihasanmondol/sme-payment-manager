using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BillPaymentManager.Models;
using BillPaymentManager.Services;
using BillPaymentManager.Services.Interfaces;
using BillPaymentManager.ViewModels.Base;

namespace BillPaymentManager.ViewModels;

/// <summary>
/// Dashboard ViewModel for displaying payment statistics
/// </summary>
public partial class DashboardViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;

    [ObservableProperty]
    private decimal _totalAmount;

    [ObservableProperty]
    private decimal _todayAmount;

    [ObservableProperty]
    private decimal _thisMonthAmount;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _todayCount;

    [ObservableProperty]
    private int _thisMonthCount;

    [ObservableProperty]
    private decimal _bKashAmount;

    [ObservableProperty]
    private decimal _nagadAmount;

    [ObservableProperty]
    private decimal _rocketAmount;

    [ObservableProperty]
    private decimal _otherAmount;

    [ObservableProperty]
    private bool _isLoading;

    public DashboardViewModel()
    {
        _databaseService = new DatabaseService();
        // Don't auto-load - let the view trigger this when ready
    }

    [RelayCommand]
    private async Task LoadData()
    {
        IsLoading = true;
        try
        {
            var stats = await _databaseService.GetStatisticsAsync();
            
            TotalAmount = stats.TotalAmount;
            TodayAmount = stats.TodayAmount;
            ThisMonthAmount = stats.ThisMonthAmount;
            TotalCount = stats.TotalCount;
            TodayCount = stats.TodayCount;
            ThisMonthCount = stats.ThisMonthCount;
            BKashAmount = stats.BKashAmount;
            NagadAmount = stats.NagadAmount;
            RocketAmount = stats.RocketAmount;
            OtherAmount = stats.OtherAmount;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
