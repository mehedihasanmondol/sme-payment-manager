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
    
    // Mobile Payment Breakdown
    public decimal BKashAmount { get; set; }
    public decimal NagadAmount { get; set; }
    public decimal RocketAmount { get; set; }
    public decimal OtherAmount { get; set; }
    
    // By Type
    public int MobilePaymentCount { get; set; }
    public decimal MobilePaymentAmount { get; set; }
    public int ElectricityTokenCount { get; set; }
    public decimal ElectricityTokenAmount { get; set; }
    
    // Electricity Specific
    public decimal TotalEnergyCost { get; set; }
    public decimal TotalMeterRent { get; set; }
    public decimal TotalDemandCharge { get; set; }
}
