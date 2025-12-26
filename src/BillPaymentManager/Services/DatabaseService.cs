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
                System.Diagnostics.Debug.WriteLine("Database created successfully");
            }
            
            // Verify the Payments table exists
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
                System.Diagnostics.Debug.WriteLine("Payments table created successfully");
            }
            
            // Final verification
            if (!_context.Database.CanConnect())
            {
                throw new Exception("Cannot connect to database");
            }
            
            System.Diagnostics.Debug.WriteLine("Database initialized successfully");
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
                MeterNumber TEXT,
                Token TEXT,
                SequenceNumber INTEGER,
                EnergyCost REAL,
                MeterRent REAL,
                DemandCharge REAL,
                VAT REAL,
                Rebate REAL,
                ArrearAmount REAL,
                VendingAmount REAL,
                TransactionId TEXT,
                PaymentDate TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                SmsText TEXT,
                CustomerName TEXT,
                Notes TEXT
            );
            
            CREATE INDEX IF NOT EXISTS IX_Payments_MeterNumber ON Payments (MeterNumber);
            CREATE INDEX IF NOT EXISTS IX_Payments_PaymentDate ON Payments (PaymentDate);
            CREATE INDEX IF NOT EXISTS IX_Payments_TransactionId ON Payments (TransactionId);
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
            return await _context.Payments.AnyAsync();
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
            System.Diagnostics.Debug.WriteLine($"Error adding payment: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine($"Error updating payment: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeletePaymentAsync(int id)
    {
        try
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting payment: {ex.Message}");
            return false;
        }
    }

    public async Task<PaymentStatistics> GetStatisticsAsync()
    {
        return await GetStatisticsByDateRangeAsync(null, null);
    }

    public async Task<PaymentStatistics> GetStatisticsByDateRangeAsync(DateTime? startDate, DateTime? endDate)
    {
        var allPayments = await _context.Payments.ToListAsync(); // In a real app, do DB-side filtering, but SQLite is small here
        
        // Absolute Metrics (Always Today/This Month relative to NOW)
        var today = DateTime.Today;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);
        
        var todayPayments = allPayments.Where(p => p.PaymentDate.Date == today).ToList();
        var monthPayments = allPayments.Where(p => p.PaymentDate >= startOfMonth).ToList();

        // Filtered Metrics (Based on Range)
        IEnumerable<Payment> filteredPayments = allPayments;
        
        if (startDate.HasValue)
        {
            filteredPayments = filteredPayments.Where(p => p.PaymentDate.Date >= startDate.Value.Date);
        }
        
        if (endDate.HasValue)
        {
            filteredPayments = filteredPayments.Where(p => p.PaymentDate.Date <= endDate.Value.Date);
        }
        
        var filteredList = filteredPayments.ToList();

        var stats = new PaymentStatistics
        {
            // Absolute Stats
            TodayAmount = todayPayments.Sum(p => p.Amount),
            TodayCount = todayPayments.Count,
            ThisMonthAmount = monthPayments.Sum(p => p.Amount),
            ThisMonthCount = monthPayments.Count,
            
            // Filtered Stats (Total becomes "Total for Range")
            TotalAmount = filteredList.Sum(p => p.Amount),
            TotalCount = filteredList.Count,
            
            // Electricity Specific (Filtered)
            TotalEnergyCost = filteredList.Sum(p => p.EnergyCost ?? 0),
            TotalMeterRent = filteredList.Sum(p => p.MeterRent ?? 0),
            TotalDemandCharge = filteredList.Sum(p => p.DemandCharge ?? 0),
            TotalVAT = filteredList.Sum(p => p.VAT ?? 0),
            TotalRebate = filteredList.Sum(p => p.Rebate ?? 0)
        };

        return stats;
    }

    public async Task<List<Payment>> SearchPaymentsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllPaymentsAsync();
        }

        return await _context.Payments
            .Where(p => 
                (p.MeterNumber != null && p.MeterNumber.Contains(searchTerm)) ||
                (p.Token != null && p.Token.Contains(searchTerm)) ||
                (p.TransactionId != null && p.TransactionId.Contains(searchTerm)) ||
                (p.CustomerName != null && p.CustomerName.Contains(searchTerm)))
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }
}
