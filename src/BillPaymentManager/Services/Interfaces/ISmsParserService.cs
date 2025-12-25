using BillPaymentManager.Models;

namespace BillPaymentManager.Services.Interfaces;

/// <summary>
/// Interface for SMS parsing service
/// </summary>
public interface ISmsParserService
{
    Payment? ParseSms(string smsText);
}
