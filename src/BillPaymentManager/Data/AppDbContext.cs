using Microsoft.EntityFrameworkCore;
using BillPaymentManager.Models;
using System.IO;

namespace BillPaymentManager.Data;

/// <summary>
/// Database context for the Bill Payment Manager
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Payment> Payments { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Get the database path
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbFolder = Path.Combine(appDataPath, "BillPaymentManager");
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(dbFolder))
            {
                Directory.CreateDirectory(dbFolder);
            }

            var dbPath = Path.Combine(dbFolder, "payments.db");
            
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MeterNumber);
            entity.HasIndex(e => e.TransactionId);
            entity.HasIndex(e => e.PaymentDate);
        });
    }
}
