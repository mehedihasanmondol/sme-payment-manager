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
    
    // Mobile Payment Stats
    [ObservableProperty]
    private int _mobilePaymentCount;
    
    [ObservableProperty]
    private decimal _mobilePaymentAmount;
    
    // Electricity Token Stats
    [ObservableProperty]
    private int _electricityTokenCount;
    
    [ObservableProperty]
    private decimal _electricityTokenAmount;
    
    [ObservableProperty]
    private decimal _totalEnergyCost;
    
    [ObservableProperty]
    private decimal _totalMeterRent;
    
    [ObservableProperty]
    private decimal _totalDemandCharge;

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
            
            // Type-specific stats
            MobilePaymentCount = stats.MobilePaymentCount;
            MobilePaymentAmount = stats.MobilePaymentAmount;
            ElectricityTokenCount = stats.ElectricityTokenCount;
            ElectricityTokenAmount = stats.ElectricityTokenAmount;
            
            // Electricity-specific
            TotalEnergyCost = stats.TotalEnergyCost;
            TotalMeterRent = stats.TotalMeterRent;
            TotalDemandCharge = stats.TotalDemandCharge;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
