namespace BillPaymentManager.Models;

/// <summary>
/// Type of payment/transaction
/// </summary>
public enum PaymentType
{
    MobilePayment,      // bKash, Nagad, Rocket payments
    ElectricityToken    // BREB/REB prepaid electricity tokens
}
