using System.Text.RegularExpressions;
using BillPaymentManager.Models;
using BillPaymentManager.Services.Interfaces;

namespace BillPaymentManager.Services;

/// <summary>
/// SMS parser service for extracting electricity token data from BREB/REB SMS
/// Supports multiple format variations
/// </summary>
public partial class SmsParserService : ISmsParserService
{
    // Flexible patterns to handle multiple SMS formats
    
    // Meter number patterns - handles both "Meter No:" and "Meter No" with/without leading zeros
    [GeneratedRegex(@"(?:Meter\s*No[:\s]*|for\s+offline\s+Meter\s+No:)(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex MeterNumberRegex();

    // Token patterns - handles "Token(s):" or "Token is"
    [GeneratedRegex(@"(?:Token(?:\(s\))?[:\s]+is\s+|Token(?:\(s\))?[:\s]+)([\d-]+)", RegexOptions.IgnoreCase)]
    private static partial Regex TokenRegex();

    // Sequence patterns - handles both "Sequence:" and "SquNo:"
    [GeneratedRegex(@"(?:Seq(?:uence)?|SquNo)[:\s]*(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex SequenceRegex();

    // Energy Cost patterns - handles "Energy Cost:", "Enrg Cost:", with optional "Tk" suffix
    [GeneratedRegex(@"(?:Enrg|Energy)\s*Cost[:\s]*([\d.,]+)(?:\s*Tk)?", RegexOptions.IgnoreCase)]
    private static partial Regex EnergyCostRegex();

    // Meter Rent patterns - with optional "Tk" suffix
    [GeneratedRegex(@"Meter\s*Rent[:\s]*([\d.,]+)(?:\s*Tk)?", RegexOptions.IgnoreCase)]
    private static partial Regex MeterRentRegex();

    // Demand Charge patterns - with optional "Tk" suffix
    [GeneratedRegex(@"Demand\s*Charge[:\s]*([\d.,]+)(?:\s*Tk)?", RegexOptions.IgnoreCase)]
    private static partial Regex DemandChargeRegex();

    // VAT patterns - with optional "Tk" suffix
    [GeneratedRegex(@"VAT[:\s]*([\d.,]+)(?:\s*Tk)?", RegexOptions.IgnoreCase)]
    private static partial Regex VATRegex();

    // Rebate patterns - handles negative values and optional "Tk" suffix
    [GeneratedRegex(@"Rebate[:\s]*(-?[\d.,]+)(?:\s*Tk)?", RegexOptions.IgnoreCase)]
    private static partial Regex RebateRegex();

    // Arrear patterns - with optional "Tk" suffix
    [GeneratedRegex(@"Arrear\s*(?:Amount)?[:\s]*([\d.,]+)(?:\s*Tk)?", RegexOptions.IgnoreCase)]
    private static partial Regex ArrearRegex();

    // Vending Amount patterns - handles "Vending Amount:", "Vending Amt:", with optional "Tk" suffix
    [GeneratedRegex(@"Vending\s*(?:Amt|Amount)[:\s]*([\d.,]+)(?:\s*Tk)?", RegexOptions.IgnoreCase)]
    private static partial Regex VendingAmountRegex();

    // Transaction ID patterns - handles "Trx ID:", "RTrx ID:"
    [GeneratedRegex(@"(?:R?Trx)\s*ID[:\s]*([A-Z0-9]+)", RegexOptions.IgnoreCase)]
    private static partial Regex TransactionIdRegex();

    // Customer Name pattern (for REB format)
    [GeneratedRegex(@"Customer\s*Name[:\s]*([^,]+)", RegexOptions.IgnoreCase)]
    private static partial Regex CustomerNameRegex();

    public Payment? ParseSms(string smsText)
    {
        if (string.IsNullOrWhiteSpace(smsText))
            return null;

        // Check if it's an electricity token SMS
        if (!IsElectricityTokenSms(smsText))
            return null;

        return ParseElectricityToken(smsText);
    }

    private bool IsElectricityTokenSms(string smsText)
    {
        return (smsText.Contains("BREB", StringComparison.OrdinalIgnoreCase) ||
                smsText.Contains("REB", StringComparison.OrdinalIgnoreCase) ||
                smsText.Contains("Prepaid Token", StringComparison.OrdinalIgnoreCase)) &&
               (smsText.Contains("Meter No", StringComparison.OrdinalIgnoreCase) ||
                smsText.Contains("Token", StringComparison.OrdinalIgnoreCase));
    }

    private Payment ParseElectricityToken(string smsText)
    {
        var payment = new Payment
        {
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

        // Extract costs (handle comma as decimal separator and remove it)
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
        payment.TransactionId = ExtractMatch(TransactionIdRegex(), smsText);

        // Extract customer name if present (REB format)
        var customerName = ExtractMatch(CustomerNameRegex(), smsText);
        if (!string.IsNullOrEmpty(customerName))
        {
            payment.CustomerName = customerName.Trim();
        }

        return payment;
    }

    private string? ExtractMatch(Regex regex, string smsText)
    {
        var match = regex.Match(smsText);
        return match.Success && match.Groups.Count > 1 ? match.Groups[1].Value.Trim() : null;
    }

    private decimal? ExtractDecimal(Regex regex, string smsText)
    {
        var match = regex.Match(smsText);
        if (match.Success && match.Groups.Count > 1)
        {
            // Remove commas and parse
            var valueStr = match.Groups[1].Value.Replace(",", "").Trim();
            if (decimal.TryParse(valueStr, out decimal value))
            {
                return value;
            }
        }
        return null;
    }
}
