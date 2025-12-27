using BillPaymentManager.Models;

namespace BillPaymentManager.Services.Interfaces;

/// <summary>
/// Interface for PDF generation service
/// </summary>
public interface IPdfService
{
    /// <summary>
    /// Generates a PDF receipt for the given payment and saves it to the specified file path
    /// </summary>
    void GeneratePdf(Payment payment, string filePath);
    
    /// <summary>
    /// Generates a PDF receipt for the given payment and returns it as a byte array
    /// </summary>
    byte[] GeneratePdfToMemory(Payment payment);
}
