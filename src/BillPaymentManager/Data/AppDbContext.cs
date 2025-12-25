using Microsoft.EntityFrameworkCore;
using BillPaymentManager.Models;
using System.IO;

namespace BillPaymentManager.Data;

/// <summary>
/// Application database context using SQLite
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Payment> Payments { get; set; } = null!;

    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Database path: AppData\Local\BillPaymentManager\payments.db
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
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Provider).HasConversion<string>();
            entity.HasIndex(e => e.TransactionId);
            entity.HasIndex(e => e.PaymentDate);
        });
    }
}
