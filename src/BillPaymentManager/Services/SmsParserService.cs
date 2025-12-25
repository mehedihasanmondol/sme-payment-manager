using System.Text.RegularExpressions;
using BillPaymentManager.Models;
using BillPaymentManager.Services.Interfaces;

namespace BillPaymentManager.Services;

/// <summary>
/// SMS parser service for extracting payment data from bKash, Nagad, and Rocket SMS
/// </summary>
public partial class SmsParserService : ISmsParserService
{
    // bKash patterns (both English and Bengali)
    [GeneratedRegex(@"(TrxID|TrxId|Transaction ID)\s*[:\s]*([A-Z0-9]+)", RegexOptions.IgnoreCase)]
    private static partial Regex BKashTrxIdRegex();

    [GeneratedRegex(@"(?:Tk|৳)\s*([\d,]+\.?\d*)", RegexOptions.IgnoreCase)]
    private static partial Regex AmountRegex();

    [GeneratedRegex(@"(\d{11})", RegexOptions.None)]
    private static partial Regex PhoneRegex();

    // Nagad patterns
    [GeneratedRegex(@"(PIN|Transaction ID)\s*[:\s]*([A-Z0-9]+)", RegexOptions.IgnoreCase)]
    private static partial Regex NagadTrxIdRegex();

    // Rocket patterns
    [GeneratedRegex(@"(Trx\s*ID|Transaction ID)\s*[:\s]*([A-Z0-9]+)", RegexOptions.IgnoreCase)]
    private static partial Regex RocketTrxIdRegex();

    public Payment? ParseSms(string smsText)
    {
        if (string.IsNullOrWhiteSpace(smsText))
            return null;

        PaymentProvider? provider = null;
        if (IsBKashSms(smsText))
            provider = PaymentProvider.bKash;
        else if (IsNagadSms(smsText))
            provider = PaymentProvider.Nagad;
        else if (IsRocketSms(smsText))
            provider = PaymentProvider.Rocket;

        if (provider == null)
            return null;

        var payment = new Payment
        {
            Provider = provider.Value,
            SmsText = smsText,
            PaymentDate = DateTime.Now,
            CreatedAt = DateTime.Now
        };

        // Extract transaction ID
        payment.TransactionId = ExtractTransactionId(smsText, provider.Value);

        // Extract amount
        payment.Amount = ExtractAmount(smsText);

        // Extract phone number
        payment.PhoneNumber = ExtractPhoneNumber(smsText);

        return payment;
    }

    public bool IsBKashSms(string smsText)
    {
        return smsText.Contains("bKash", StringComparison.OrdinalIgnoreCase) ||
               smsText.Contains("বিকাশ");
    }

    public bool IsNagadSms(string smsText)
    {
        return smsText.Contains("Nagad", StringComparison.OrdinalIgnoreCase) ||
               smsText.Contains("নগদ");
    }

    public bool IsRocketSms(string smsText)
    {
        return smsText.Contains("Rocket", StringComparison.OrdinalIgnoreCase) ||
               smsText.Contains("রকেট");
    }

    private string? ExtractTransactionId(string smsText, PaymentProvider provider)
    {
        Regex regex = provider switch
        {
            PaymentProvider.bKash => BKashTrxIdRegex(),
            PaymentProvider.Nagad => NagadTrxIdRegex(),
            PaymentProvider.Rocket => RocketTrxIdRegex(),
            _ => BKashTrxIdRegex()
        };

        var match = regex.Match(smsText);
        return match.Success && match.Groups.Count > 2 ? match.Groups[2].Value : null;
    }

    private decimal ExtractAmount(string smsText)
    {
        var match = AmountRegex().Match(smsText);
        if (match.Success && match.Groups.Count > 1)
        {
            var amountStr = match.Groups[1].Value.Replace(",", "");
            if (decimal.TryParse(amountStr, out decimal amount))
            {
                return amount;
            }
        }
        return 0;
    }

    private string? ExtractPhoneNumber(string smsText)
    {
        var matches = PhoneRegex().Matches(smsText);
        // Return the first 11-digit number found (Bangladesh phone numbers are 11 digits)
        foreach (Match match in matches)
        {
            var phone = match.Groups[1].Value;
            if (phone.StartsWith("01")) // Bangladesh mobile numbers start with 01
            {
                return phone;
            }
        }
        return matches.Count > 0 ? matches[0].Groups[1].Value : null;
    }
}
