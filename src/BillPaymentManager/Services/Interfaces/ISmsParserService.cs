using BillPaymentManager.Models;

namespace BillPaymentManager.Services.Interfaces;

/// <summary>
/// SMS parser service interface for extracting payment data
/// </summary>
public interface ISmsParserService
{
    Payment? ParseSms(string smsText);
    bool IsBKashSms(string smsText);
    bool IsNagadSms(string smsText);
    bool IsRocketSms(string smsText);
}
