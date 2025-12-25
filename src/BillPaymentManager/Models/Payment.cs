using System.ComponentModel.DataAnnotations;

namespace BillPaymentManager.Models;

/// <summary>
/// Payment record entity
/// </summary>
public class Payment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public PaymentProvider Provider { get; set; }

    [MaxLength(100)]
    public string? TransactionId { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    public DateTime PaymentDate { get; set; }

    public DateTime CreatedAt { get; set; }

    [MaxLength(1000)]
    public string? SmsText { get; set; }

    [MaxLength(200)]
    public string? CustomerName { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
