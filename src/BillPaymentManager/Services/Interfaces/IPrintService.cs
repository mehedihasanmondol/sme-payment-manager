using BillPaymentManager.Models;

namespace BillPaymentManager.Services.Interfaces;

/// <summary>
/// Print service interface for generating receipts
/// </summary>
public interface IPrintService
{
    bool PrintReceipt(Payment payment);
    bool PrintMultipleReceipts(List<Payment> payments);
    string GenerateReceiptText(Payment payment);
}
