namespace BillPaymentManager.Models;

/// <summary>
/// Payment statistics for dashboard
/// </summary>
public class PaymentStatistics
{
    public decimal TotalAmount { get; set; }
    public decimal TodayAmount { get; set; }
    public decimal ThisMonthAmount { get; set; }
    public int TotalCount { get; set; }
    public int TodayCount { get; set; }
    public int ThisMonthCount { get; set; }
    public decimal BKashAmount { get; set; }
    public decimal NagadAmount { get; set; }
    public decimal RocketAmount { get; set; }
    public decimal OtherAmount { get; set; }
}
