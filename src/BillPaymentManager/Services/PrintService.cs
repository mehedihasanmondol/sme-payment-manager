using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using BillPaymentManager.Models;
using BillPaymentManager.Services.Interfaces;

namespace BillPaymentManager.Services;

/// <summary>
/// Print service for generating and printing payment receipts
/// </summary>
public class PrintService : IPrintService
{
    private Payment? _currentPayment;
    private readonly Font _titleFont = new("Arial", 14, FontStyle.Bold);
    private readonly Font _normalFont = new("Arial", 10);
    private readonly Font _boldFont = new("Arial", 10, FontStyle.Bold);

    public bool PrintReceipt(Payment payment)
    {
        try
        {
            _currentPayment = payment;
            var printDocument = new PrintDocument();
            printDocument.PrintPage += PrintDocument_PrintPage;
            printDocument.Print();
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Print error: {ex.Message}");
            return false;
        }
    }

    public bool PrintMultipleReceipts(List<Payment> payments)
    {
        foreach (var payment in payments)
        {
            if (!PrintReceipt(payment))
                return false;
        }
        return true;
    }

    public string GenerateReceiptText(Payment payment)
    {
        var sb = new StringBuilder();
        sb.AppendLine("========================================");
        sb.AppendLine("         PAYMENT RECEIPT");
        sb.AppendLine("========================================");
        sb.AppendLine();
        sb.AppendLine($"Transaction ID: {payment.TransactionId}");
        sb.AppendLine($"Provider:       {payment.Provider}");
        sb.AppendLine($"Amount:         ৳{payment.Amount:N2}");
        sb.AppendLine($"Phone:          {payment.PhoneNumber}");
        sb.AppendLine($"Date:           {payment.PaymentDate:dd/MM/yyyy hh:mm tt}");
        
        if (!string.IsNullOrEmpty(payment.CustomerName))
            sb.AppendLine($"Customer:       {payment.CustomerName}");
        
        if (!string.IsNullOrEmpty(payment.Notes))
        {
            sb.AppendLine();
            sb.AppendLine($"Notes: {payment.Notes}");
        }
        
        sb.AppendLine();
        sb.AppendLine("========================================");
        sb.AppendLine("        Thank you for your payment!");
        sb.AppendLine("========================================");
        
        return sb.ToString();
    }

    private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
    {
        if (_currentPayment == null || e.Graphics == null)
            return;

        float yPosition = 50;
        float leftMargin = 50;

        // Title
        e.Graphics.DrawString("PAYMENT RECEIPT", _titleFont, Brushes.Black, leftMargin, yPosition);
        yPosition += 40;

        // Draw line
        e.Graphics.DrawLine(Pens.Black, leftMargin, yPosition, 300, yPosition);
        yPosition += 20;

        // Payment details
        e.Graphics.DrawString("Transaction ID:", _boldFont, Brushes.Black, leftMargin, yPosition);
        e.Graphics.DrawString(_currentPayment.TransactionId ?? "N/A", _normalFont, Brushes.Black, leftMargin + 120, yPosition);
        yPosition += 25;

        e.Graphics.DrawString("Provider:", _boldFont, Brushes.Black, leftMargin, yPosition);
        e.Graphics.DrawString(_currentPayment.Provider.ToString(), _normalFont, Brushes.Black, leftMargin + 120, yPosition);
        yPosition += 25;

        e.Graphics.DrawString("Amount:", _boldFont, Brushes.Black, leftMargin, yPosition);
        e.Graphics.DrawString($"৳{_currentPayment.Amount:N2}", _titleFont, Brushes.Black, leftMargin + 120, yPosition);
        yPosition += 30;

        e.Graphics.DrawString("Phone:", _boldFont, Brushes.Black, leftMargin, yPosition);
        e.Graphics.DrawString(_currentPayment.PhoneNumber ?? "N/A", _normalFont, Brushes.Black, leftMargin + 120, yPosition);
        yPosition += 25;

        e.Graphics.DrawString("Date:", _boldFont, Brushes.Black, leftMargin, yPosition);
        e.Graphics.DrawString(_currentPayment.PaymentDate.ToString("dd/MM/yyyy hh:mm tt"), _normalFont, Brushes.Black, leftMargin + 120, yPosition);
        yPosition += 30;

        if (!string.IsNullOrEmpty(_currentPayment.CustomerName))
        {
            e.Graphics.DrawString("Customer:", _boldFont, Brushes.Black, leftMargin, yPosition);
            e.Graphics.DrawString(_currentPayment.CustomerName, _normalFont, Brushes.Black, leftMargin + 120, yPosition);
            yPosition += 25;
        }

        // Draw line
        yPosition += 10;
        e.Graphics.DrawLine(Pens.Black, leftMargin, yPosition, 300, yPosition);
        yPosition += 20;

        // Thank you message
        e.Graphics.DrawString("Thank you for your payment!", _normalFont, Brushes.Black, leftMargin, yPosition);
    }
}
