using System.Windows;
using BillPaymentManager.Models;
using System.Drawing;
using System.Drawing.Printing;

namespace BillPaymentManager.Views;

/// <summary>
/// Print preview window for electricity token receipts
/// </summary>
public partial class PrintPreviewWindow : Window
{
    private readonly Payment _payment;
    
    public PrintPreviewWindow(Payment payment)
    {
        InitializeComponent();
        _payment = payment;
        DataContext = payment;
    }
    
    private void PrintButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            PrintDocument printDoc = new PrintDocument();
            printDoc.PrintPage += PrintPage;
            
            // Show print dialog
            var printDialog = new System.Windows.Forms.PrintDialog();
            printDialog.Document = printDoc;
            
            if (printDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                printDoc.Print();
                MessageBox.Show("Receipt sent to printer successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Print failed: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
    
    private void PrintPage(object sender, PrintPageEventArgs e)
    {
        if (e.Graphics == null) return;
        
        float yPos = 30;
        float leftMargin = 50;
        float centerX = e.PageBounds.Width / 2;
        
        // Fonts
        var titleFont = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold);
        var headerFont = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold);
        var normalFont = new System.Drawing.Font("Arial", 10);
        var smallFont = new System.Drawing.Font("Arial", 9);
        var tokenFont = new System.Drawing.Font("Courier New", 14, System.Drawing.FontStyle.Bold);
        
        // Helper function for centered text
        void DrawCenteredText(string text, System.Drawing.Font font, System.Drawing.Brush brush, ref float y, float spacing = 20)
        {
            var size = e.Graphics.MeasureString(text, font);
            e.Graphics.DrawString(text, font, brush, centerX - (size.Width / 2), y);
            y += spacing;
        }
        
        // Helper function for line
        void DrawLine(ref float y, float margin = 30)
        {
            e.Graphics.DrawLine(System.Drawing.Pens.Black, leftMargin, y, e.PageBounds.Width - leftMargin, y);
            y += margin;
        }
        
        // Header
        DrawCenteredText("তাসকিন ডিজিটাল স্টুডিও", titleFont, System.Drawing.Brushes.Black, ref yPos, 25);
        DrawCenteredText("মেহেরী বাজার, ডিভ অফিসের নিচে", normalFont, System.Drawing.Brushes.Black, ref yPos, 18);
        DrawCenteredText("মোবাইল: ০১৮১৫৫৫৫৫৯৮", normalFont, System.Drawing.Brushes.Black, ref yPos, 18);
        DrawCenteredText("প্রোঃ মোঃ মাহাবুব", normalFont, System.Drawing.Brushes.Black, ref yPos, 18);
        DrawCenteredText("রূপসা, রূপগঞ্জ, নারায়ণগঞ্জ", smallFont, System.Drawing.Brushes.Black, ref yPos, 18);
        DrawCenteredText($"তারিখ: {_payment.PaymentDate:dd-MM-yyyy hh:mm:ss tt}", smallFont, System.Drawing.Brushes.Black, ref yPos, 15);
        
        DrawLine(ref yPos, 10);
        
        // Main Title
        DrawCenteredText("পল্লী বিদ্যুৎ প্রিপেইড রশিদ", headerFont, System.Drawing.Brushes.Black, ref yPos, 15);
        
        DrawLine(ref yPos, 15);
        
        // Details Section
        var detailsLeftMargin = leftMargin + 10;
        var detailsRightMargin = e.PageBounds.Width - leftMargin - 10;
        
        void DrawDetail(string label, string value, ref float y)
        {
            e.Graphics.DrawString(label, normalFont, System.Drawing.Brushes.Black, detailsLeftMargin, y);
            var valueSize = e.Graphics.MeasureString($": {value}", normalFont);
            e.Graphics.DrawString($": {value}", normalFont, System.Drawing.Brushes.Black, 
                detailsRightMargin - valueSize.Width, y);
            y += 20;
        }
        
        DrawDetail("মিটার নম্বর", _payment.MeterNumber ?? "N/A", ref yPos);
        DrawDetail("গ্রাহক নাম", _payment.CustomerName ?? "N/A", ref yPos);
        DrawDetail("সিকুয়েন্স নম্বর", _payment.SequenceNumber?.ToString() ?? "N/A", ref yPos);
        
        yPos += 5;
        DrawLine(ref yPos, 15);
        
        // Cost Breakdown
        DrawDetail("Energy Cost", $"{_payment.EnergyCost:N2}", ref yPos);
        DrawDetail("Demand Charge", $"{_payment.DemandCharge:N2}", ref yPos);
        DrawDetail("Meter Rent", $"{_payment.MeterRent:N2}", ref yPos);
        DrawDetail("VAT", $"{_payment.VAT:N2}", ref yPos);
        DrawDetail("Rebate", $"{_payment.Rebate:N2}", ref yPos);
        DrawDetail("Transaction ID", _payment.TransactionId ?? "(Not Provided)", ref yPos);
        
        yPos += 5;
        
        // Total Amount
        e.Graphics.DrawString("মোট পরিমাণ", headerFont, System.Drawing.Brushes.Black, detailsLeftMargin, yPos);
        var totalSize = e.Graphics.MeasureString($": {_payment.VendingAmount:N2}", headerFont);
        e.Graphics.DrawString($": {_payment.VendingAmount:N2}", headerFont, System.Drawing.Brushes.Black, 
            detailsRightMargin - totalSize.Width, yPos);
        yPos += 30;
        
        DrawLine(ref yPos, 15);
        
        // Token Section
        DrawCenteredText("Token", headerFont, System.Drawing.Brushes.Black, ref yPos, 10);
        DrawCenteredText(_payment.Token ?? "N/A", tokenFont, System.Drawing.Brushes.Black, ref yPos, 15);
        
        DrawLine(ref yPos, 15);
        
        // Footer
        DrawCenteredText("ধন্যবাদ, তাসকিন ডিজিটাল স্টুডিও।", normalFont, System.Drawing.Brushes.Black, ref yPos, 15);
        DrawCenteredText("Developed by: Abu Kahar Siddiq", smallFont, System.Drawing.Brushes.Gray, ref yPos, 10);
    }
}
