namespace BillPaymentManager.Models;

/// <summary>
/// Model for storing receipt content settings (business info, labels, etc.)
/// </summary>
public class ReceiptSettings
{
    /// <summary>
    /// Business name (header)
    /// </summary>
    public string BusinessName { get; set; } = "xyz ডিজিটাল স্টুডিও";
    
    /// <summary>
    /// Business address line 1
    /// </summary>
    public string Address { get; set; } = "মেহেরী বাজার,  জানিনা";
    
    /// <summary>
    /// Phone number
    /// </summary>
    public string PhoneNumber { get; set; } = "০৪৫৩৫৫৫৯৮";
    
    /// <summary>
    /// Owner/Proprietor name
    /// </summary>
    public string OwnerName { get; set; } = "প্রোঃ মোঃ অজানা";
    
    /// <summary>
    /// Location/Area
    /// </summary>
    public string Location { get; set; } = "রূপসা, কি হবে, নারায়ণগঞ্জ";
    
    /// <summary>
    /// Main receipt title
    /// </summary>
    public string ReceiptTitle { get; set; } = "পল্লী বিদ্যুৎ প্রিপেইড রশিদ";
    
    /// <summary>
    /// Footer thank you message
    /// </summary>
    public string FooterText { get; set; } = "ধন্যবাদ, নাজানা ডিজিটাল স্টুডিও।";
    
    /// <summary>
    /// Developer credit line
    /// </summary>
    public string DeveloperCredit { get; set; } = "Developed by: Mehedi Hasan Mondol";
}
