using System.Text.RegularExpressions;
using BillPaymentManager.Models;
using BillPaymentManager.Services.Interfaces;

namespace BillPaymentManager.Services;

/// <summary>
/// SMS parser service for extracting payment data from mobile payments and electricity tokens
/// </summary>
public partial class SmsParserService : ISmsParserService
{
    // Mobile Payment: bKash patterns
    [GeneratedRegex(@"(TrxID|TrxId|Transaction ID)\s*[:\s]*([A-Z0-9]+)", RegexOptions.IgnoreCase)]
    private static partial Regex BKashTrxIdRegex();

    [GeneratedRegex(@"(?:Tk|৳)\s*([\d,]+\.?\d*)", RegexOptions.IgnoreCase)]
    private static partial Regex AmountRegex();

    [GeneratedRegex(@"(\d{11})", RegexOptions.None)]
    private static partial Regex PhoneRegex();

    // Mobile Payment: Nagad patterns
    [GeneratedRegex(@"(PIN|Transaction ID)\s*[:\s]*([A-Z0-9]+)", RegexOptions.IgnoreCase)]
    private static partial Regex NagadTrxIdRegex();

    // Mobile Payment: Rocket patterns
    [GeneratedRegex(@"(Trx\s*ID|Transaction ID)\s*[:\s]*([A-Z0-9]+)", RegexOptions.IgnoreCase)]
    private static partial Regex RocketTrxIdRegex();

    // Electricity Token patterns
    [GeneratedRegex(@"Meter\s*No[:\s]*(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex MeterNumberRegex();

    [GeneratedRegex(@"Token[s]*[:\s]*([\d-]+)", RegexOptions.IgnoreCase)]
    private static partial Regex TokenRegex();

    [GeneratedRegex(@"(?:Seq(?:uence)?|SquNo)[:\s]*(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex SequenceRegex();

    [GeneratedRegex(@"(?:Enrg|Energy)\s*Cost[:\s]*([\d.]+)", RegexOptions.IgnoreCase)]
    private static partial Regex EnergyCostRegex();

    [GeneratedRegex(@"Meter\s*Rent[:\s]*([\d.]+)", RegexOptions.IgnoreCase)]
    private static partial Regex MeterRentRegex();

    [GeneratedRegex(@"Demand\s*Charge[:\s]*([\d.]+)", RegexOptions.IgnoreCase)]
    private static partial Regex DemandChargeRegex();

    [GeneratedRegex(@"VAT[:\s]*([\d.]+)", RegexOptions.IgnoreCase)]
    private static partial Regex VATRegex();

    [GeneratedRegex(@"Rebate[:\s]*(-?[\d.]+)", RegexOptions.IgnoreCase)]
    private static partial Regex RebateRegex();

    [GeneratedRegex(@"Arrear\s*Amount[:\s]*([\d.]+)", RegexOptions.IgnoreCase)]
    private static partial Regex ArrearRegex();

    [GeneratedRegex(@"Vending\s*(?:Amt|Amount)[:\s]*([\d.]+)", RegexOptions.IgnoreCase)]
    private static partial Regex VendingAmountRegex();

    [GeneratedRegex(@"(?:Trx|RTrx)\s*ID[:\s]*([A-Z0-9]+)", RegexOptions.IgnoreCase)]
    private static partial Regex ElectricityTrxIdRegex();

    public Payment? ParseSms(string smsText)
    {
        if (string.IsNullOrWhiteSpace(smsText))
            return null;

        // Check if it's an electricity token first
        if (IsElectricityTokenSms(smsText))
        {
            return ParseElectricityToken(smsText);
        }

        // Check for mobile payments
        PaymentProvider? provider = null;
        if (IsBKashSms(smsText))
            provider = PaymentProvider.bKash;
        else if (IsNagadSms(smsText))
            provider = PaymentProvider.Nagad;
        else if (IsRocketSms(smsText))
            provider = PaymentProvider.Rocket;

        if (provider == null)
            return null;

        return ParseMobilePayment(smsText, provider.Value);
    }

    public bool IsElectricityTokenSms(string smsText)
    {
        return (smsText.Contains("BREB", StringComparison.OrdinalIgnoreCase) ||
                smsText.Contains("REB", StringComparison.OrdinalIgnoreCase) ||
                smsText.Contains("Prepaid Token", StringComparison.OrdinalIgnoreCase)) &&
               (smsText.Contains("Meter No", StringComparison.OrdinalIgnoreCase) ||
                smsText.Contains("Token", StringComparison.OrdinalIgnoreCase));
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

    private Payment ParseElectricityToken(string smsText)
    {
        var payment = new Payment
        {
            Type = PaymentType.ElectricityToken,
            Provider = PaymentProvider.Other, // Not applicable for electricity
            SmsText = smsText,
            PaymentDate = DateTime.Now,
            CreatedAt = DateTime.Now
        };

        // Extract meter number
        payment.MeterNumber = ExtractMatch(MeterNumberRegex(), smsText);

        // Extract token
        payment.Token = ExtractMatch(TokenRegex(), smsText);

        // Extract sequence number
        var seqStr = ExtractMatch(SequenceRegex(), smsText);
        if (!string.IsNullOrEmpty(seqStr) && int.TryParse(seqStr, out int seq))
        {
            payment.SequenceNumber = seq;
        }

        // Extract costs
        payment.EnergyCost = ExtractDecimal(EnergyCostRegex(), smsText);
        payment.MeterRent = ExtractDecimal(MeterRentRegex(), smsText);
        payment.DemandCharge = ExtractDecimal(DemandChargeRegex(), smsText);
        payment.VAT = ExtractDecimal(VATRegex(), smsText);
        payment.Rebate = ExtractDecimal(RebateRegex(), smsText);
        payment.ArrearAmount = ExtractDecimal(ArrearRegex(), smsText);
        payment.VendingAmount = ExtractDecimal(VendingAmountRegex(), smsText);

        // Total amount is vending amount
        payment.Amount = payment.VendingAmount ?? 0;

        // Extract transaction ID
        payment.TransactionId = ExtractMatch(ElectricityTrxIdRegex(), smsText);

        return payment;
    }

    private Payment ParseMobilePayment(string smsText, PaymentProvider provider)
    {
        var payment = new Payment
        {
            Type = PaymentType.MobilePayment,
            Provider = provider,
            SmsText = smsText,
            PaymentDate = DateTime.Now,
            CreatedAt = DateTime.Now
        };

        // Extract transaction ID
        payment.TransactionId = ExtractTransactionId(smsText, provider);

        // Extract amount
        payment.Amount = ExtractMobilePaymentAmount(smsText);

        // Extract phone number
        payment.PhoneNumber = ExtractPhoneNumber(smsText);

        return payment;
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

    private decimal ExtractMobilePaymentAmount(string smsText)
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

    private string? ExtractMatch(Regex regex, string smsText)
    {
        var match = regex.Match(smsText);
        return match.Success && match.Groups.Count > 1 ? match.Groups[1].Value : null;
    }

    private decimal? ExtractDecimal(Regex regex, string smsText)
    {
        var match = regex.Match(smsText);
        if (match.Success && match.Groups.Count > 1)
        {
            var valueStr = match.Groups[1].Value.Replace(",", "");
            if (decimal.TryParse(valueStr, out decimal value))
            {
                return value;
            }
        }
        return null;
    }
}
