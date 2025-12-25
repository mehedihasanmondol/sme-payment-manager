namespace BillPaymentManager.Models;

/// <summary>
/// Payment statistics for dashboard
/// </summary>
public class PaymentStatistics
{
    // Overall Statistics
    public decimal TotalAmount { get; set; }
    public decimal TodayAmount { get; set; }
    public decimal ThisMonthAmount { get; set; }
    public int TotalCount { get; set; }
    public int TodayCount { get; set; }
    public int ThisMonthCount { get; set; }
    
    // Electricity Token Specific
    public decimal TotalEnergyCost { get; set; }
    public decimal TotalMeterRent { get; set; }
    public decimal TotalDemandCharge { get; set; }
    public decimal TotalVAT { get; set; }
    public decimal TotalRebate { get; set; }
}
