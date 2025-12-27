using System.IO;
using System.Text.Json;
using BillPaymentManager.Models;

namespace BillPaymentManager.Services;

/// <summary>
/// Service for managing persistent printer settings
/// </summary>
public static class PrintSettingsManager
{
    private static readonly string SettingsFolder;
    private static readonly string SettingsFilePath;
    private static readonly string ReceiptSettingsFilePath;
    
    static PrintSettingsManager()
    {
        // Get LocalApplicationData folder
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        SettingsFolder = Path.Combine(appDataPath, "BillPaymentManager");
        SettingsFilePath = Path.Combine(SettingsFolder, "printsettings.json");
        ReceiptSettingsFilePath = Path.Combine(SettingsFolder, "receiptsettings.json");
    }
    
    /// <summary>
    /// Load print settings from disk. Returns default settings if file doesn't exist.
    /// </summary>
    public static PrintSettings LoadSettings()
    {
        try
        {
            if (!File.Exists(SettingsFilePath))
            {
                return new PrintSettings(); // Return default A4 settings
            }
            
            var json = File.ReadAllText(SettingsFilePath);
            var settings = JsonSerializer.Deserialize<PrintSettings>(json);
            return settings ?? new PrintSettings();
        }
        catch
        {
            // If any error occurs (corrupted file, etc.), return defaults
            return new PrintSettings();
        }
    }
    
    /// <summary>
    /// Save print settings to disk
    /// </summary>
    public static void SaveSettings(PrintSettings settings)
    {
        try
        {
            // Ensure folder exists
            if (!Directory.Exists(SettingsFolder))
            {
                Directory.CreateDirectory(SettingsFolder);
            }
            
            // Serialize with indentation for readability
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(settings, options);
            
            File.WriteAllText(SettingsFilePath, json);
        }
        catch (Exception ex)
        {
            // Log or handle error (for now, silently fail to not disrupt printing)
            System.Diagnostics.Debug.WriteLine($"Failed to save print settings: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Load receipt content settings from disk. Returns default settings if file doesn't exist.
    /// </summary>
    public static ReceiptSettings LoadReceiptSettings()
    {
        try
        {
            if (!File.Exists(ReceiptSettingsFilePath))
            {
                // Create default settings file
                var defaultSettings = new ReceiptSettings();
                SaveReceiptSettings(defaultSettings);
                return defaultSettings;
            }
            
            var json = File.ReadAllText(ReceiptSettingsFilePath);
            var settings = JsonSerializer.Deserialize<ReceiptSettings>(json);
            return settings ?? new ReceiptSettings();
        }
        catch
        {
            // If any error occurs (corrupted file, etc.), return defaults
            return new ReceiptSettings();
        }
    }
    
    /// <summary>
    /// Save receipt content settings to disk
    /// </summary>
    public static void SaveReceiptSettings(ReceiptSettings settings)
    {
        try
        {
            // Ensure folder exists
            if (!Directory.Exists(SettingsFolder))
            {
                Directory.CreateDirectory(SettingsFolder);
            }
            
            // Serialize with indentation for readability
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(settings, options);
            
            File.WriteAllText(ReceiptSettingsFilePath, json);
        }
        catch (Exception ex)
        {
            // Log or handle error
            System.Diagnostics.Debug.WriteLine($"Failed to save receipt settings: {ex.Message}");
        }
    }
}

