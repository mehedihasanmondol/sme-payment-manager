using System.Drawing;
using System.Drawing.Printing;
using BillPaymentManager.Models;
using BillPaymentManager.Services.Interfaces;

namespace BillPaymentManager.Services;

/// <summary>
/// Print service for generating electricity token receipts
/// </summary>
public class PrintService : IPrintService
{
    private Payment? _currentPayment;
    private Font _titleFont = new Font("Arial", 16, FontStyle.Bold);
    private Font _headerFont = new Font("Arial", 12, FontStyle.Bold);
    private Font _normalFont = new Font("Arial", 10);
    private Font _tokenFont = new Font("Courier New", 12, FontStyle.Bold);

    public void PrintReceipt(Payment payment)
    {
        _currentPayment = payment;

        PrintDocument printDoc = new PrintDocument();
        printDoc.PrintPage += new PrintPageEventHandler(PrintPage);

        try
        {
            printDoc.Print();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Print error: {ex.Message}");
            System.Windows.MessageBox.Show(
                $"Print failed: {ex.Message}",
                "Print Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    private void PrintPage(object sender, PrintPageEventArgs e)
    {
        if (_currentPayment == null || e.Graphics == null)
            return;

        float yPos = 50;
        float leftMargin = 50;
        string receiptText = GenerateReceiptText(_currentPayment);

        // Title
        e.Graphics.DrawString("ELECTRICITY TOKEN RECEIPT", _titleFont, Brushes.Black, leftMargin, yPos);
        yPos += 40;

        // Separator Line
        e.Graphics.DrawLine(Pens.Black, leftMargin, yPos, leftMargin + 400, yPos);
        yPos += 20;

        // Receipt Details
        string[] lines = receiptText.Split('\n');
        foreach (string line in lines)
        {
            if (line.StartsWith("===="))
            {
                e.Graphics.DrawLine(Pens.Black, leftMargin, yPos, leftMargin + 400, yPos);
                yPos += 15;
            }
            else if (line.StartsWith("TOKEN:"))
            {
                e.Graphics.DrawString(line, _tokenFont, Brushes.DarkBlue, leftMargin, yPos);
                yPos += 30;
            }
            else if (line.StartsWith("METER") || line.StartsWith("VENDING"))
            {
                e.Graphics.DrawString(line, _headerFont, Brushes.Black, leftMargin, yPos);
                yPos += 25;
            }
            else
            {
                e.Graphics.DrawString(line, _normalFont, Brushes.Black, leftMargin, yPos);
                yPos += 20;
            }
        }

        // Footer
        yPos += 20;
        e.Graphics.DrawString($"Printed: {DateTime.Now:dd/MM/yyyy HH:mm}", _normalFont, Brushes.Gray, leftMargin, yPos);
    }

    private string GenerateReceiptText(Payment payment)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine($"Date: {payment.PaymentDate:dd/MM/yyyy HH:mm}");
        sb.AppendLine($"Transaction ID: {payment.TransactionId ?? "N/A"}");
        sb.AppendLine("=====================================");
        
        sb.AppendLine($"METER NUMBER: {payment.MeterNumber ?? "N/A"}");
        sb.AppendLine($"Sequence: {payment.SequenceNumber?.ToString() ?? "N/A"}");
        sb.AppendLine("");
        
        sb.AppendLine($"TOKEN: {payment.Token ?? "N/A"}");
        sb.AppendLine("=====================================");
        sb.AppendLine("");
        
        sb.AppendLine("COST BREAKDOWN:");
        sb.AppendLine($"  Energy Cost:      ৳{payment.EnergyCost:N2}");
        sb.AppendLine($"  Meter Rent:       ৳{payment.MeterRent:N2}");
        sb.AppendLine($"  Demand Charge:    ৳{payment.DemandCharge:N2}");
        sb.AppendLine($"  VAT:              ৳{payment.VAT:N2}");
        sb.AppendLine($"  Rebate:           ৳{payment.Rebate:N2}");
        
        if (payment.ArrearAmount.HasValue && payment.ArrearAmount.Value > 0)
        {
            sb.AppendLine($"  Arrear:           ৳{payment.ArrearAmount:N2}");
        }
        
        sb.AppendLine("=====================================");
        sb.AppendLine($"VENDING AMOUNT:     ৳{payment.VendingAmount:N2}");
        sb.AppendLine("=====================================");
        
        if (!string.IsNullOrWhiteSpace(payment.CustomerName))
        {
            sb.AppendLine("");
            sb.AppendLine($"Customer: {payment.CustomerName}");
        }
        
        if (!string.IsNullOrWhiteSpace(payment.Notes))
        {
            sb.AppendLine($"Notes: {payment.Notes}");
        }

        return sb.ToString();
    }
}
