using System.Windows;
using BillPaymentManager.Models;
using BillPaymentManager.Services;
using BillPaymentManager.Services.Interfaces;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;

namespace BillPaymentManager.Views;

/// <summary>
/// Print preview window for electricity token receipts
/// </summary>
public partial class PrintPreviewWindow : Window
{
    private readonly Payment _payment;
    private readonly IPdfService _pdfService;
    
    public PrintPreviewWindow(Payment payment)
    {
        InitializeComponent();
        _payment = payment;
        _pdfService = new PdfService();
        DataContext = payment;
    }
    
    private void PrintButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            PrintDocument printDoc = new PrintDocument();
            printDoc.PrintPage += PrintPage;
            
            // Load saved settings
            var settings = PrintSettingsManager.LoadSettings();
            
            // Apply saved printer if available
            if (!string.IsNullOrEmpty(settings.PrinterName))
            {
                // Check if the saved printer still exists
                var availablePrinters = PrinterSettings.InstalledPrinters.Cast<string>();
                if (availablePrinters.Contains(settings.PrinterName))
                {
                    printDoc.PrinterSettings.PrinterName = settings.PrinterName;
                }
            }
            
            // Apply saved page settings
            // Try to use the saved PaperKind if it's a standard size, otherwise use custom dimensions
            var paperKind = (System.Drawing.Printing.PaperKind)settings.PaperKind;
            if (paperKind == System.Drawing.Printing.PaperKind.Custom)
            {
                printDoc.DefaultPageSettings.PaperSize = new PaperSize(settings.PaperSizeName, settings.PaperWidth, settings.PaperHeight);
            }
            else
            {
                // For standard sizes, find matching PaperSize from printer's available sizes
                var matchingPaper = printDoc.PrinterSettings.PaperSizes.Cast<PaperSize>()
                    .FirstOrDefault(ps => ps.Kind == paperKind);
                if (matchingPaper != null)
                {
                    printDoc.DefaultPageSettings.PaperSize = matchingPaper;
                }
                else
                {
                    // Fallback to custom if standard size not available
                    printDoc.DefaultPageSettings.PaperSize = new PaperSize(settings.PaperSizeName, settings.PaperWidth, settings.PaperHeight);
                }
            }
            printDoc.DefaultPageSettings.Margins = new Margins(settings.LeftMargin, settings.RightMargin, 
                settings.TopMargin, settings.BottomMargin);
            
            // Direct print
            printDoc.Print();
            
            MessageBox.Show("Receipt sent to printer successfully!", "Success", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Print failed: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void PrinterSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            PrintDocument printDoc = new PrintDocument();
            printDoc.PrintPage += PrintPage;
            
            // Load saved settings to pre-populate dialog
            var settings = PrintSettingsManager.LoadSettings();
            
            // Apply saved printer if available
            if (!string.IsNullOrEmpty(settings.PrinterName))
            {
                var availablePrinters = PrinterSettings.InstalledPrinters.Cast<string>();
                if (availablePrinters.Contains(settings.PrinterName))
                {
                    printDoc.PrinterSettings.PrinterName = settings.PrinterName;
                }
            }
            
            // Apply saved page settings to pre-populate dialog
            var paperKind = (System.Drawing.Printing.PaperKind)settings.PaperKind;
            if (paperKind == System.Drawing.Printing.PaperKind.Custom)
            {
                printDoc.DefaultPageSettings.PaperSize = new PaperSize(settings.PaperSizeName, settings.PaperWidth, settings.PaperHeight);
            }
            else
            {
                var matchingPaper = printDoc.PrinterSettings.PaperSizes.Cast<PaperSize>()
                    .FirstOrDefault(ps => ps.Kind == paperKind);
                if (matchingPaper != null)
                {
                    printDoc.DefaultPageSettings.PaperSize = matchingPaper;
                }
                else
                {
                    printDoc.DefaultPageSettings.PaperSize = new PaperSize(settings.PaperSizeName, settings.PaperWidth, settings.PaperHeight);
                }
            }
            printDoc.DefaultPageSettings.Margins = new Margins(settings.LeftMargin, settings.RightMargin, 
                settings.TopMargin, settings.BottomMargin);
            
            // Show print dialog
            var printDialog = new System.Windows.Forms.PrintDialog();
            printDialog.Document = printDoc;
            
            if (printDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Save the selected settings for future use
                var paperSize = printDoc.DefaultPageSettings.PaperSize;
                var newSettings = new PrintSettings
                {
                    PrinterName = printDoc.PrinterSettings.PrinterName,
                    PaperSizeName = paperSize.PaperName,
                    PaperKind = (int)paperSize.Kind,
                    PaperWidth = paperSize.Width,
                    PaperHeight = paperSize.Height,
                    LeftMargin = printDoc.DefaultPageSettings.Margins.Left,
                    RightMargin = printDoc.DefaultPageSettings.Margins.Right,
                    TopMargin = printDoc.DefaultPageSettings.Margins.Top,
                    BottomMargin = printDoc.DefaultPageSettings.Margins.Bottom
                };
                PrintSettingsManager.SaveSettings(newSettings);
                
                printDoc.Print();
                MessageBox.Show("Receipt sent to printer successfully!\nYour printer settings have been saved.", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Settings/Print failed: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
    
    private void DownloadPdfButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Create suggested filename
            var meterNumber = _payment.MeterNumber?.Replace(" ", "_") ?? "Unknown";
            var date = _payment.PaymentDate.ToString("yyyyMMdd_HHmmss");
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
                _pdfService.GeneratePdf(_payment, saveFileDialog.FileName);
                
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
