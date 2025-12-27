using System.Drawing;
using System.Drawing.Printing;
using BillPaymentManager.Models;
using BillPaymentManager.Services.Interfaces;
using BillPaymentManager.Views;

namespace BillPaymentManager.Services;

/// <summary>
/// Print service for generating electricity token receipts
/// </summary>
public class PrintService : IPrintService
{
    public void ShowPrintPreview(Payment payment)
    {
        var previewWindow = new PrintPreviewWindow(payment);
        previewWindow.ShowDialog();
    }
    
    public void PrintReceipt(Payment payment)
    {
        var receiptSettings = PrintSettingsManager.LoadReceiptSettings();
        PrintDocument printDoc = new PrintDocument();
        printDoc.PrintPage += (sender, e) => PrintPage(payment, receiptSettings, e);

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

    private void PrintPage(Payment payment, ReceiptSettings settings, PrintPageEventArgs e)
    {
        if (e.Graphics == null) return;
        
        float yPos = 30;
        float leftMargin = 50;
        float centerX = e.PageBounds.Width / 2;
        
        // Fonts
        var titleFont = new Font("Arial", 16, FontStyle.Bold);
        var headerFont = new Font("Arial", 12, FontStyle.Bold);
        var normalFont = new Font("Arial", 10);
        var smallFont = new Font("Arial", 9);
        var tokenFont = new Font("Courier New", 14, FontStyle.Bold);
        
        // Helper function for centered text
        void DrawCenteredText(string text, Font font, Brush brush, ref float y, float spacing = 20)
        {
            var size = e.Graphics.MeasureString(text, font);
            e.Graphics.DrawString(text, font, brush, centerX - (size.Width / 2), y);
            y += spacing;
        }
        
        // Helper function for line
        void DrawLine(ref float y, float margin = 30)
        {
            e.Graphics.DrawLine(Pens.Black, leftMargin, y, e.PageBounds.Width - leftMargin, y);
            y += margin;
        }
        
        // Header
        DrawCenteredText(settings.BusinessName, titleFont, Brushes.Black, ref yPos, 30);
        DrawCenteredText(settings.Address, normalFont, Brushes.Black, ref yPos, 20);
        DrawCenteredText($"মোবাইল: {settings.PhoneNumber}", normalFont, Brushes.Black, ref yPos, 20);
        DrawCenteredText(settings.OwnerName, normalFont, Brushes.Black, ref yPos, 20);
        DrawCenteredText(settings.Location, smallFont, Brushes.Black, ref yPos, 20);
        DrawCenteredText($"তারিখ: {payment.PaymentDate:dd-MM-yyyy hh:mm:ss tt}", smallFont, Brushes.Black, ref yPos, 20);
        
        DrawLine(ref yPos, 15);
        
        // Main Title
        DrawCenteredText(settings.ReceiptTitle, headerFont, Brushes.Black, ref yPos, 20);
        
        DrawLine(ref yPos, 20);
        
        // Details Section
        var detailsLeftMargin = leftMargin + 10;
        var detailsRightMargin = e.PageBounds.Width - leftMargin - 10;
        
        void DrawDetail(string label, string value, ref float y)
        {
            e.Graphics.DrawString(label, normalFont, Brushes.Black, detailsLeftMargin, y);
            var valueSize = e.Graphics.MeasureString($": {value}", normalFont);
            e.Graphics.DrawString($": {value}", normalFont, Brushes.Black, 
                detailsRightMargin - valueSize.Width, y);
            y += 25; // Increased from 20 to 25 for better spacing
        }
        
        DrawDetail("মিটার নম্বর", payment.MeterNumber ?? "N/A", ref yPos);
        DrawDetail("গ্রাহক নাম", payment.CustomerName ?? "N/A", ref yPos);
        DrawDetail("সিকুয়েন্স নম্বর", payment.SequenceNumber?.ToString() ?? "N/A", ref yPos);
        
        yPos += 10; // Increased spacing
        DrawLine(ref yPos, 20);
        
        // Cost Breakdown
        DrawDetail("Energy Cost", $"{payment.EnergyCost:N2}", ref yPos);
        DrawDetail("Demand Charge", $"{payment.DemandCharge:N2}", ref yPos);
        DrawDetail("Meter Rent", $"{payment.MeterRent:N2}", ref yPos);
        DrawDetail("VAT", $"{payment.VAT:N2}", ref yPos);
        DrawDetail("Rebate", $"{payment.Rebate:N2}", ref yPos);
        DrawDetail("Transaction ID", payment.TransactionId ?? "(Not Provided)", ref yPos);
        
        yPos += 10; // Added spacing before total
        
        // Total Amount
        e.Graphics.DrawString("মোট পরিমাণ", headerFont, Brushes.Black, detailsLeftMargin, yPos);
        var totalSize = e.Graphics.MeasureString($": {payment.VendingAmount:N2}", headerFont);
        e.Graphics.DrawString($": {payment.VendingAmount:N2}", headerFont, Brushes.Black, 
            detailsRightMargin - totalSize.Width, yPos);
        yPos += 35; // Increased spacing after total
        
        DrawLine(ref yPos, 20);
        
        // Token Section
        DrawCenteredText("Token", headerFont, Brushes.Black, ref yPos, 15);
        DrawCenteredText(payment.Token ?? "N/A", tokenFont, Brushes.Black, ref yPos, 20);
        
        DrawLine(ref yPos, 20);
        
        // Footer
        DrawCenteredText(settings.FooterText, normalFont, Brushes.Black, ref yPos, 20);
        DrawCenteredText(settings.DeveloperCredit, smallFont, Brushes.Gray, ref yPos, 15);
    }
}
