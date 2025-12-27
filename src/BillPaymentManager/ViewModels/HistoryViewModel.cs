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
    private readonly IPdfService _pdfService;

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

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalRecords;

    private List<Payment> _allFilteredPayments = new();

    public HistoryViewModel()
    {
        _databaseService = new DatabaseService();
        _printService = new PrintService();
        _pdfService = new PdfService();
        
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

            _allFilteredPayments = payments;
            TotalRecords = payments.Count;
            TotalPages = (int)Math.Ceiling((double)TotalRecords / PageSize);
            
            // Reset to page 1 when loading new data
            CurrentPage = 1;
            LoadPage();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void LoadPage()
    {
        var pagedPayments = _allFilteredPayments
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        Payments.Clear();
        foreach (var payment in pagedPayments)
        {
            Payments.Add(payment);
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
    private void DownloadPdf()
    {
        if (SelectedPayment == null)
        {
            MessageBox.Show("Please select a payment record first.", "No Selection", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            // Create suggested filename
            var meterNumber = SelectedPayment.MeterNumber?.Replace(" ", "_") ?? "Unknown";
            var date = SelectedPayment.PaymentDate.ToString("yyyyMMdd_HHmmss");
            var suggestedFileName = $"Receipt_{meterNumber}_{date}.pdf";
            
            // Show save file dialog
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = suggestedFileName,
                DefaultExt = ".pdf",
                AddExtension = true,
                Title = "Save Receipt as PDF"
            };
            
            if (saveFileDialog.ShowDialog() == true)
            {
                // Generate and save PDF
                _pdfService.GeneratePdf(SelectedPayment, saveFileDialog.FileName);
                
                MessageBox.Show($"PDF saved successfully!\n\nLocation: {saveFileDialog.FileName}", 
                    "Success", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save PDF: {ex.Message}", 
                "Error", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
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

    [RelayCommand]
    private void FirstPage()
    {
        if (CurrentPage != 1)
        {
            CurrentPage = 1;
            LoadPage();
        }
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            LoadPage();
        }
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            LoadPage();
        }
    }

    [RelayCommand]
    private void LastPage()
    {
        if (CurrentPage != TotalPages)
        {
            CurrentPage = TotalPages;
            LoadPage();
        }
    }
}
