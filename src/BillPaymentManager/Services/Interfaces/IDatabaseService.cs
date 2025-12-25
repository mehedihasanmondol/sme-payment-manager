using BillPaymentManager.Models;

namespace BillPaymentManager.Services.Interfaces;

/// <summary>
/// Interface for database operations
/// </summary>
public interface IDatabaseService
{
    Task<bool> IsDatabaseInitializedAsync();
    Task<List<Payment>> GetAllPaymentsAsync();
    Task<List<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Payment?> GetPaymentByIdAsync(int id);
    Task<bool> AddPaymentAsync(Payment payment);
    Task<bool> UpdatePaymentAsync(Payment payment);
    Task<bool> DeletePaymentAsync(int id);
    Task<PaymentStatistics> GetStatisticsAsync();
    Task<List<Payment>> SearchPaymentsAsync(string searchTerm);
}
