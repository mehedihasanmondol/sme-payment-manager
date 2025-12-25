using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BillPaymentManager.Models;
using BillPaymentManager.Services;
using BillPaymentManager.Services.Interfaces;
using BillPaymentManager.ViewModels.Base;

namespace BillPaymentManager.ViewModels;

/// <summary>
/// ViewModel for electricity token history with filtering capabilities
/// </summary>
public partial class HistoryViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IPrintService _printService;

    [ObservableProperty]
    private ObservableCollection<Payment> _payments = new();

    [ObservableProperty]
    private Payment? _selectedPayment;

    [ObservableProperty]
    private DateTime? _startDate;

    [ObservableProperty]
    private DateTime? _endDate;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public HistoryViewModel()
    {
        _databaseService = new DatabaseService();
        _printService = new PrintService();
        
        // Set default date range to current month
        StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        EndDate = DateTime.Now;
    }

    [RelayCommand]
    private async Task LoadData()
    {
        IsLoading = true;
        try
        {
            List<Payment> payments;

            if (StartDate.HasValue && EndDate.HasValue)
            {
                payments = await _databaseService.GetPaymentsByDateRangeAsync(StartDate.Value, EndDate.Value);
            }
            else
            {
                payments = await _databaseService.GetAllPaymentsAsync();
            }

            // Apply text search filter if provided
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                payments = payments.Where(p =>
                    (p.TransactionId?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.MeterNumber?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.Token?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.CustomerName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            Payments.Clear();
            foreach (var payment in payments)
            {
                Payments.Add(payment);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ApplyFilter()
    {
        LoadDataCommand.Execute(null);
    }

    [RelayCommand]
    private void ClearFilter()
    {
        StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        EndDate = DateTime.Now;
        SearchText = string.Empty;
        LoadDataCommand.Execute(null);
    }

    [RelayCommand]
    private void PrintSelected()
    {
        if (SelectedPayment != null)
        {
            _printService.ShowPrintPreview(SelectedPayment);
        }
    }

    [RelayCommand]
    private async Task DeleteSelected()
    {
        if (SelectedPayment == null)
            return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete this token?\n\nMeter: {SelectedPayment.MeterNumber}\nToken: {SelectedPayment.Token}\nAmount: ৳{SelectedPayment.Amount:N2}",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            var success = await _databaseService.DeletePaymentAsync(SelectedPayment.Id);
            if (success)
            {
                MessageBox.Show("Token deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadData();
            }
            else
            {
                MessageBox.Show("Failed to delete token", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
