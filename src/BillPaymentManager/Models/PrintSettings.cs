namespace BillPaymentManager.Models;

/// <summary>
/// Model for storing printer configuration settings
/// </summary>
public class PrintSettings
{
    /// <summary>
    /// Name of the selected printer
    /// </summary>
    public string PrinterName { get; set; } = string.Empty;
    
    /// <summary>
    /// Paper size name (e.g., "A4", "Letter", "Custom")
    /// </summary>
    public string PaperSizeName { get; set; } = "A4";
    
    /// <summary>
    /// Paper kind enum value (as integer for JSON serialization)
    /// </summary>
    public int PaperKind { get; set; } = 9; // A4 = 9 in PaperKind enum
    
    /// <summary>
    /// Paper width in hundredths of an inch (e.g., 827 for A4)
    /// </summary>
    public int PaperWidth { get; set; } = 827; // A4 default
    
    /// <summary>
    /// Paper height in hundredths of an inch (e.g., 1169 for A4)
    /// </summary>
    public int PaperHeight { get; set; } = 1169; // A4 default
    
    /// <summary>
    /// Left margin in hundredths of an inch
    /// </summary>
    public int LeftMargin { get; set; } = 50;
    
    /// <summary>
    /// Right margin in hundredths of an inch
    /// </summary>
    public int RightMargin { get; set; } = 50;
    
    /// <summary>
    /// Top margin in hundredths of an inch
    /// </summary>
    public int TopMargin { get; set; } = 50;
    
    /// <summary>
    /// Bottom margin in hundredths of an inch
    /// </summary>
    public int BottomMargin { get; set; } = 50;
}
