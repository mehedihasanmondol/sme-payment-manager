using CommunityToolkit.Mvvm.ComponentModel;
using BillPaymentManager.ViewModels.Base;
using System.IO;

namespace BillPaymentManager.ViewModels;

/// <summary>
/// ViewModel for application settings
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _databasePath = string.Empty;

    [ObservableProperty]
    private string _applicationVersion = "1.0.0";

    public SettingsViewModel()
    {
        // Get database location
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dbFolder = Path.Combine(appDataPath, "BillPaymentManager");
        DatabasePath = Path.Combine(dbFolder, "payments.db");
    }
}
