using System.ComponentModel.DataAnnotations;

namespace BillPaymentManager.Models;

/// <summary>
/// Payment record entity - supports both mobile payments and electricity tokens
/// </summary>
public class Payment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public PaymentType Type { get; set; }

    [Required]
    public decimal Amount { get; set; }

    // Mobile Payment Fields
    [Required]
    public PaymentProvider Provider { get; set; }

    [MaxLength(100)]
    public string? TransactionId { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    // Electricity Token Fields
    [MaxLength(50)]
    public string? MeterNumber { get; set; }

    [MaxLength(100)]
    public string? Token { get; set; }

    public int? SequenceNumber { get; set; }

    public decimal? EnergyCost { get; set; }

    public decimal? MeterRent { get; set; }

    public decimal? DemandCharge { get; set; }

    public decimal? VAT { get; set; }

    public decimal? Rebate { get; set; }

    public decimal? ArrearAmount { get; set; }

    public decimal? VendingAmount { get; set; }

    // Common Fields
    public DateTime PaymentDate { get; set; }

    public DateTime CreatedAt { get; set; }

    [MaxLength(2000)]
    public string? SmsText { get; set; }

    [MaxLength(200)]
    public string? CustomerName { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
