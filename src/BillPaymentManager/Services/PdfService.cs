using System.IO;
using System.Reflection;
using BillPaymentManager.Models;
using BillPaymentManager.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BillPaymentManager.Services;

/// <summary>
/// PDF generation service for electricity token receipts with Bangla text support
/// </summary>
public class PdfService : IPdfService
{
    private const string BanglaFontName = "Noto Sans Bengali";
    
    static PdfService()
    {
        // Configure QuestPDF license (Community license for free use)
        QuestPDF.Settings.License = LicenseType.Community;
        
        // Register Bangla font from embedded resources
        RegisterBanglaFont();
    }
    
    private static void RegisterBanglaFont()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fontResourceName = "BillPaymentManager.Fonts.NotoSansBengali-Regular.ttf";
            
            using var stream = assembly.GetManifestResourceStream(fontResourceName);
            if (stream != null)
            {
                QuestPDF.Drawing.FontManager.RegisterFont(stream);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to register Bangla font: {ex.Message}");
        }
    }
    
    public void GeneratePdf(Payment payment, string filePath)
    {
        var document = CreateDocument(payment);
        document.GeneratePdf(filePath);
    }
    
    public byte[] GeneratePdfToMemory(Payment payment)
    {
        var document = CreateDocument(payment);
        return document.GeneratePdf();
    }
    
    private Document CreateDocument(Payment payment)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(BanglaFontName));
                
                page.Content().Column(column =>
                {
                    column.Spacing(5);
                    
                    // Header
                    column.Item().AlignCenter().Text("তাসকিন ডিজিটাল স্টুডিও")
                        .FontSize(20).Bold().FontFamily(BanglaFontName);
                    
                    column.Item().AlignCenter().Text("মেহেরী বাজার, ডিভ অফিসের নিচে")
                        .FontSize(12).FontFamily(BanglaFontName);
                    
                    column.Item().AlignCenter().Text("মোবাইল: ০১৮১৫৫৫৫৫৯৮")
                        .FontSize(12).FontFamily(BanglaFontName);
                    
                    column.Item().AlignCenter().Text("প্রোঃ মোঃ মাহাবুব")
                        .FontSize(12).FontFamily(BanglaFontName);
                    
                    column.Item().AlignCenter().Text("রূপসা, রূপগঞ্জ, নারায়ণগঞ্জ")
                        .FontSize(11).FontFamily(BanglaFontName);
                    
                    column.Item().AlignCenter().Text($"তারিখ: {payment.PaymentDate:dd-MM-yyyy hh:mm:ss tt}")
                        .FontSize(11).FontFamily(BanglaFontName);
                    
                    // Separator
                    column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Black);
                    
                    // Main Title
                    column.Item().AlignCenter().PaddingVertical(5).Text("পল্লী বিদ্যুৎ প্রিপেইড রশিদ")
                        .FontSize(16).Bold().FontFamily(BanglaFontName);
                    
                    // Separator
                    column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Black);
                    
                    // Details Section
                    column.Item().PaddingVertical(10).Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Row(r =>
                            {
                                r.RelativeItem().Text("মিটার নম্বর").FontFamily(BanglaFontName);
                                r.AutoItem().Text($": {payment.MeterNumber ?? "N/A"}");
                            });
                            col.Item().PaddingTop(5).Row(r =>
                            {
                                r.RelativeItem().Text("গ্রাহক নাম").FontFamily(BanglaFontName);
                                r.AutoItem().Text($": {payment.CustomerName ?? "N/A"}").FontFamily(BanglaFontName);
                            });
                            col.Item().PaddingTop(5).Row(r =>
                            {
                                r.RelativeItem().Text("সিকুয়েন্স নম্বর").FontFamily(BanglaFontName);
                                r.AutoItem().Text($": {payment.SequenceNumber?.ToString() ?? "N/A"}");
                            });
                        });
                    });
                    
                    // Separator
                    column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Black);
                    
                    // Cost Breakdown
                    column.Item().PaddingVertical(5).Column(col =>
                    {
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Energy Cost");
                            r.AutoItem().Text($": {payment.EnergyCost:N2}");
                        });
                        col.Item().PaddingTop(3).Row(r =>
                        {
                            r.RelativeItem().Text("Demand Charge");
                            r.AutoItem().Text($": {payment.DemandCharge:N2}");
                        });
                        col.Item().PaddingTop(3).Row(r =>
                        {
                            r.RelativeItem().Text("Meter Rent");
                            r.AutoItem().Text($": {payment.MeterRent:N2}");
                        });
                        col.Item().PaddingTop(3).Row(r =>
                        {
                            r.RelativeItem().Text("VAT");
                            r.AutoItem().Text($": {payment.VAT:N2}");
                        });
                        col.Item().PaddingTop(3).Row(r =>
                        {
                            r.RelativeItem().Text("Rebate");
                            r.AutoItem().Text($": {payment.Rebate:N2}");
                        });
                        col.Item().PaddingTop(3).Row(r =>
                        {
                            r.RelativeItem().Text("Transaction ID");
                            r.AutoItem().Text($": {payment.TransactionId ?? "(Not Provided)"}");
                        });
                    });
                    
                    // Total Amount
                    column.Item().PaddingVertical(10).Row(row =>
                    {
                        row.RelativeItem().Text("মোট পরিমাণ").FontSize(14).Bold().FontFamily(BanglaFontName);
                        row.AutoItem().Text($": {payment.VendingAmount:N2}").FontSize(14).Bold();
                    });
                    
                    // Separator
                    column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Black);
                    
                    // Token Section
                    column.Item().AlignCenter().PaddingTop(10).Text("Token")
                        .FontSize(16).Bold();
                    
                    column.Item().AlignCenter().PaddingVertical(5).Text(payment.Token ?? "N/A")
                        .FontSize(14).Bold().FontFamily("Courier New");
                    
                    // Separator
                    column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Black);
                    
                    // Footer
                    column.Item().AlignCenter().Text("ধন্যবাদ, তাসকিন ডিজিটাল স্টুডিও।")
                        .FontSize(12).FontFamily(BanglaFontName);
                    
                    column.Item().AlignCenter().PaddingTop(5).Text("Developed by: Abu Kahar Siddiq")
                        .FontSize(9).FontColor(Colors.Grey.Medium);
                });
            });
        });
    }
}
