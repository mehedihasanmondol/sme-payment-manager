using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BillPaymentManager.Models;
using BillPaymentManager.Services;
using BillPaymentManager.Services.Interfaces;
using BillPaymentManager.ViewModels.Base;

namespace BillPaymentManager.ViewModels;

/// <summary>
/// Dashboard ViewModel for displaying electricity token statistics
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
    private bool _isLoading;
    
    // Electricity Token Stats
    [ObservableProperty]
    private decimal _totalEnergyCost;
    
    [ObservableProperty]
    private decimal _totalMeterRent;
    
    [ObservableProperty]
    private decimal _totalDemandCharge;
    
    [ObservableProperty]
    private decimal _totalVAT;
    
    [ObservableProperty]
    private decimal _totalRebate;

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
            
            // Electricity-specific
            TotalEnergyCost = stats.TotalEnergyCost;
            TotalMeterRent = stats.TotalMeterRent;
            TotalDemandCharge = stats.TotalDemandCharge;
            TotalVAT = stats.TotalVAT;
            TotalRebate = stats.TotalRebate;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
