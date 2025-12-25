using BillPaymentManager.Models;

namespace BillPaymentManager.Services.Interfaces;

/// <summary>
/// Interface for printing receipts
/// </summary>
public interface IPrintService
{
    void PrintReceipt(Payment payment);
    void ShowPrintPreview(Payment payment);
}
