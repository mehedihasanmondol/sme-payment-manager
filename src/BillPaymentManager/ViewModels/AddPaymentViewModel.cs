using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BillPaymentManager.Models;
using BillPaymentManager.Services;
using BillPaymentManager.Services.Interfaces;
using BillPaymentManager.ViewModels.Base;

namespace BillPaymentManager.ViewModels;

/// <summary>
/// ViewModel for adding new payments by parsing SMS
/// </summary>
public partial class AddPaymentViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private readonly ISmsParserService _smsParserService;
    private readonly IPrintService _printService;

    [ObservableProperty]
    private string _smsText = string.Empty;

    [ObservableProperty]
    private Payment? _parsedPayment;

    [ObservableProperty]
    private bool _isParsed;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public AddPaymentViewModel()
    {
        _databaseService = new DatabaseService();
        _smsParserService = new SmsParserService();
        _printService = new PrintService();
    }

    [RelayCommand]
    private async Task ParseAndSave()
    {
        if (string.IsNullOrWhiteSpace(SmsText))
        {
            StatusMessage = "Please paste SMS text";
            return;
        }

        ParsedPayment = _smsParserService.ParseSms(SmsText);
        
        if (ParsedPayment != null)
        {
            IsParsed = true;
            
            // Auto-save
            IsSaving = true;
            try
            {
                var success = await _databaseService.AddPaymentAsync(ParsedPayment);
                if (success)
                {
                    StatusMessage = "Payment saved successfully! Opening preview...";
                    // Show preview
                    _printService.ShowPrintPreview(ParsedPayment);
                }
                else
                {
                    StatusMessage = "Parsed but failed to save to database";
                    MessageBox.Show("Failed to save payment to database", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                IsSaving = false;
            }
        }
        else
        {
            IsParsed = false;
            StatusMessage = "Unable to parse SMS. Please check the format.";
        }
    }

    [RelayCommand]
    private async Task SavePayment()
    {
        if (ParsedPayment == null)
        {
            StatusMessage = "No payment data to save";
            return;
        }

        IsSaving = true;
        try
        {
            var success = await _databaseService.AddPaymentAsync(ParsedPayment);
            
            if (success)
            {
                StatusMessage = "Payment saved successfully!";
                MessageBox.Show("Payment saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Ask if user wants to print
                var result = MessageBox.Show("Do you want to print the receipt?", "Print Receipt", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    _printService.ShowPrintPreview(ParsedPayment);
                }
                
                // Reset form
                ResetForm();
            }
            else
            {
                StatusMessage = "Failed to save payment";
                MessageBox.Show("Failed to save payment to database", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void ResetForm()
    {
        SmsText = string.Empty;
        ParsedPayment = null;
        IsParsed = false;
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private void PrintReceipt()
    {
        if (ParsedPayment != null)
        {
            _printService.ShowPrintPreview(ParsedPayment);
            StatusMessage = "Receipt preview opened";
        }
    }
}
