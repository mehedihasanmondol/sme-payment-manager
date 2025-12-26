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
        
        // Default to Today
        StartDate = DateTime.Today;
        EndDate = DateTime.Today;
        UpdateDateRangeHint();
        
        // Don't auto-load - let the view trigger this when ready
    }

    [ObservableProperty]
    private DateTime? _startDate;

    [ObservableProperty]
    private DateTime? _endDate;

    [ObservableProperty]
    private string _dateRangeHint = "(All Time Data)";

    [RelayCommand]
    private async Task LoadData()
    {
        IsLoading = true;
        try
        {
            // 1. Fetch Global Stats (Today, Month, All Time) - Unaffected by filter
            var globalStats = await _databaseService.GetStatisticsByDateRangeAsync(null, null);
            
            TotalAmount = globalStats.TotalAmount;
            TotalCount = globalStats.TotalCount;
            TodayAmount = globalStats.TodayAmount;
            TodayCount = globalStats.TodayCount;
            ThisMonthAmount = globalStats.ThisMonthAmount;
            ThisMonthCount = globalStats.ThisMonthCount;
            
            // 2. Fetch Business Analytics - Affected by filter
            var filteredStats = await _databaseService.GetStatisticsByDateRangeAsync(StartDate, EndDate);
            
            // Electricity-specific (Business Analytics)
            TotalEnergyCost = filteredStats.TotalEnergyCost;
            TotalMeterRent = filteredStats.TotalMeterRent;
            TotalDemandCharge = filteredStats.TotalDemandCharge;
            TotalVAT = filteredStats.TotalVAT;
            TotalRebate = filteredStats.TotalRebate;
            
            UpdateDateRangeHint();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task FilterData()
    {
        await LoadData();
    }

    [RelayCommand]
    private async Task ClearFilter()
    {
        StartDate = null;
        EndDate = null;
        await LoadData();
    }
    
    private void UpdateDateRangeHint()
    {
        if (StartDate == null && EndDate == null)
        {
            DateRangeHint = "(All Time Data)";
        }
        else if (StartDate != null && EndDate != null)
        {
            DateRangeHint = $"({StartDate:dd/MM/yyyy} - {EndDate:dd/MM/yyyy})";
        }
        else if (StartDate != null)
        {
             DateRangeHint = $"(From {StartDate:dd/MM/yyyy})";
        }
        else if (EndDate != null)
        {
             DateRangeHint = $"(Until {EndDate:dd/MM/yyyy})";
        }
    }
}
