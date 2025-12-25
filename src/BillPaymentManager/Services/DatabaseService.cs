using Microsoft.EntityFrameworkCore;
using BillPaymentManager.Data;
using BillPaymentManager.Models;
using BillPaymentManager.Services.Interfaces;

namespace BillPaymentManager.Services;

/// <summary>
/// Database service implementation using EF Core and SQLite
/// </summary>
public class DatabaseService : IDatabaseService
{
    private readonly AppDbContext _context;

    public DatabaseService()
    {
        _context = new AppDbContext();
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        try
        {
            var dbPath = GetDatabasePath();
            System.Diagnostics.Debug.WriteLine($"Database path: {dbPath}");
            
            // Ensure database and tables are created
            var created = _context.Database.EnsureCreated();
            
            if (created)
            {
                System.Diagnostics.Debug.WriteLine("Database created successfully via EnsureCreated");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Database already exists");
            }
            
            // Verify the Payments table exists by trying to query it
            try
            {
                var testQuery = _context.Payments.Take(1).ToList();
                System.Diagnostics.Debug.WriteLine("Payments table verified - database is ready");
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.Message.Contains("no such table"))
            {
                // Table doesn't exist, create it manually
                System.Diagnostics.Debug.WriteLine("Payments table not found, creating manually...");
                CreatePaymentsTableManually();
                System.Diagnostics.Debug.WriteLine("Payments table created manually");
            }
            
            // Final verification
            if (!_context.Database.CanConnect())
            {
                throw new Exception("Cannot connect to database");
            }
            
            System.Diagnostics.Debug.WriteLine("Database initialized and verified successfully");
        }
        catch (Exception ex)
        {
            var errorMessage = $"Database initialization failed: {ex.Message}\n\n" +
                              $"Database Path: {GetDatabasePath()}\n\n" +
                              $"Error Details: {ex.ToString()}";
            
            System.Diagnostics.Debug.WriteLine(errorMessage);
            System.Windows.MessageBox.Show(
                errorMessage,
                "Database Initialization Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
            
            throw;
        }
    }
    
    private void CreatePaymentsTableManually()
    {
        var createTableSql = @"
            CREATE TABLE IF NOT EXISTS Payments (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Amount REAL NOT NULL,
                Provider TEXT NOT NULL,
                TransactionId TEXT,
                PhoneNumber TEXT,
                PaymentDate TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                SmsText TEXT,
                CustomerName TEXT,
                Notes TEXT
            );
            
            CREATE INDEX IF NOT EXISTS IX_Payments_TransactionId ON Payments (TransactionId);
            CREATE INDEX IF NOT EXISTS IX_Payments_PaymentDate ON Payments (PaymentDate);
        ";
        
        _context.Database.ExecuteSqlRaw(createTableSql);
    }
    
    private string GetDatabasePath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dbFolder = System.IO.Path.Combine(appDataPath, "BillPaymentManager");
        
        // Ensure directory exists
        if (!System.IO.Directory.Exists(dbFolder))
        {
            System.IO.Directory.CreateDirectory(dbFolder);
            System.Diagnostics.Debug.WriteLine($"Created database directory: {dbFolder}");
        }
        
        return System.IO.Path.Combine(dbFolder, "payments.db");
    }

    public async Task<bool> IsDatabaseInitializedAsync()
    {
        try
        {
            return await _context.Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<Payment>> GetAllPaymentsAsync()
    {
        return await _context.Payments
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<List<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Payments
            .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<List<Payment>> GetPaymentsByProviderAsync(PaymentProvider provider)
    {
        return await _context.Payments
            .Where(p => p.Provider == provider)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<Payment?> GetPaymentByIdAsync(int id)
    {
        return await _context.Payments.FindAsync(id);
    }

    public async Task<bool> AddPaymentAsync(Payment payment)
    {
        try
        {
            payment.CreatedAt = DateTime.Now;
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Add payment error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdatePaymentAsync(Payment payment)
    {
        try
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Update payment error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeletePaymentAsync(int id)
    {
        try
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return false;

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Delete payment error: {ex.Message}");
            return false;
        }
    }

    public async Task<PaymentStatistics> GetStatisticsAsync()
    {
        var allPayments = await _context.Payments.ToListAsync();
        var today = DateTime.Today;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);

        var stats = new PaymentStatistics
        {
            TotalAmount = allPayments.Sum(p => p.Amount),
            TotalCount = allPayments.Count,
            TodayAmount = allPayments.Where(p => p.PaymentDate.Date == today).Sum(p => p.Amount),
            TodayCount = allPayments.Count(p => p.PaymentDate.Date == today),
            ThisMonthAmount = allPayments.Where(p => p.PaymentDate >= startOfMonth).Sum(p => p.Amount),
            ThisMonthCount = allPayments.Count(p => p.PaymentDate >= startOfMonth),
            BKashAmount = allPayments.Where(p => p.Provider == PaymentProvider.bKash).Sum(p => p.Amount),
            NagadAmount = allPayments.Where(p => p.Provider == PaymentProvider.Nagad).Sum(p => p.Amount),
            RocketAmount = allPayments.Where(p => p.Provider == PaymentProvider.Rocket).Sum(p => p.Amount),
            OtherAmount = allPayments.Where(p => p.Provider == PaymentProvider.Other).Sum(p => p.Amount)
        };

        return stats;
    }
}
