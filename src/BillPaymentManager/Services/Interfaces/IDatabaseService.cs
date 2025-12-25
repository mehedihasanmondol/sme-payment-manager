using BillPaymentManager.Models;

namespace BillPaymentManager.Services.Interfaces;

/// <summary>
/// Database service interface for payment operations
/// </summary>
public interface IDatabaseService
{
    Task<List<Payment>> GetAllPaymentsAsync();
    Task<List<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<List<Payment>> GetPaymentsByProviderAsync(PaymentProvider provider);
    Task<Payment?> GetPaymentByIdAsync(int id);
    Task<bool> AddPaymentAsync(Payment payment);
    Task<bool> UpdatePaymentAsync(Payment payment);
    Task<bool> DeletePaymentAsync(int id);
    Task<PaymentStatistics> GetStatisticsAsync();
    Task<bool> IsDatabaseInitializedAsync();
}
