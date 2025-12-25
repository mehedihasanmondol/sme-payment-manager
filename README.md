# Bill Payment Manager

A Windows desktop application for managing mobile payment receipts from bKash, Nagad, and Rocket in Bangladesh.

## Features

- ✅ **Automatic SMS Parsing**: Extract payment data from bKash, Nagad, and Rocket SMS
- ✅ **Dashboard**: Real-time statistics showing today's, monthly, and all-time payment summaries
- ✅ **Payment History**: Advanced filtering by date range, provider, transaction ID, phone number, or customer name
- ✅ **Receipt Printing**: Generate and print formatted receipts for payment records
- ✅ **SQLite Database**: Local data storage with automatic database creation and migrations
- ✅ **MVVM Architecture**: Clean separation of concerns using the MVVM pattern

## Technology Stack

- **.NET 8** - Cross-platform framework
- **WPF** - Windows Presentation Foundation for UI
- **MVVM** - Model-View-ViewModel pattern with CommunityToolkit.Mvvm
- **Entity Framework Core** - ORM for SQLite database access
- **SQLite** - Lightweight embedded database

## Prerequisites

- .NET 8.0 SDK or later
- Windows 7 or later
- VS Code with C# Dev Kit extension (recommended)

## Installation & Setup

### 1. Clone or download this repository

```bash
cd c:\Users\Love Station\Documents\sme-payment-manager
```

### 2. Restore dependencies

```bash
dotnet restore
```

### 3. Build the project

```bash
dotnet build
```

### 4. Run the application

```bash
dotnet run --project src/BillPaymentManager/BillPaymentManager.csproj
```

Or press **F5** in VS Code to run with debugging.

## Database

The SQLite database is automatically created at:
```
%LOCALAPPDATA%\BillPaymentManager\payments.db
```

Example path:
```
C:\Users\YourName\AppData\Local\BillPaymentManager\payments.db
```

The database is created automatically on first run with Entity Framework migrations.

## Usage

### Adding a Payment

1. Click **"➕ Add Payment"** from the navigation menu
2. Paste the SMS text from bKash, Nagad, or Rocket into the text box
3. Click **"🔍 Parse SMS"** to extract payment details
4. Review the extracted information (you can add customer name and notes)
5. Click **"💾 Save Payment"** to save to database
6. Optionally print a receipt

### Viewing Statistics

Click **"📊 Dashboard"** to see:
- Today's total amount and transaction count
- This month's total amount and transaction count
- All-time totals
- Amount breakdown by provider (bKash, Nagad, Rocket, Other)

### Payment History

Click **"📜 History"** to:
- View all payment records in a sortable table
- Filter by date range
- Filter by payment provider
- Search by transaction ID, phone number, or customer name
- Print selected receipts
- Delete payment records

### Settings

Click **"⚙️ Settings"** to view:
- Application version
- Database location
- Application information

## Sample SMS Formats

**bKash:**
```
You have received Tk 1,500.00 from 01711XXXXXX. TrxID ABC123XYZ at 25/12/2025 10:30 PM.
```

**Nagad:**
```
You have received ৳1,500.00 from 01711XXXXXX. Transaction ID: ABC123XYZ Date: 25/12/2025 10:30 PM.
```

**Rocket:**
```
You have received Tk 1,500.00 from 01711XXXXXX via Rocket. Trx ID: ABC123XYZ on 25/12/2025.
```

## Project Structure

```
BillPaymentManager/
├── src/
│   └── BillPaymentManager/
│       ├── Models/                 # Data models (Payment, PaymentProvider, etc.)
│       ├── ViewModels/             # MVVM ViewModels
│       │   ├── Base/              # Base classes (ViewModelBase)
│       │   ├── MainViewModel.cs
│       │   ├── DashboardViewModel.cs
│       │   ├── AddPaymentViewModel.cs
│       │   ├── HistoryViewModel.cs
│       │   └── SettingsViewModel.cs
│       ├── Views/                  # XAML views
│       │   ├── DashboardView.xaml
│       │   ├── AddPaymentView.xaml
│       │   ├── HistoryView.xaml
│       │   └── SettingsView.xaml
│       ├── Services/               # Business logic services
│       │   ├── Interfaces/        # Service interfaces
│       │   ├── DatabaseService.cs
│       │   ├── SmsParserService.cs
│       │   └── PrintService.cs
│       ├── Data/                   # Database context and migrations
│       │   └── AppDbContext.cs
│       ├── Converters/             # XAML value converters
│       ├── Resources/              # Styles and resources
│       │   └── Styles/
│       │       └── AppStyles.xaml
│       ├── MainWindow.xaml         # Main application window
│       └── App.xaml                # Application entry point
└── tests/
    └── BillPaymentManager.Tests/  # Unit tests
```

## NuGet Packages Used

- **Microsoft.EntityFrameworkCore** (8.0.11) - ORM framework
- **Microsoft.EntityFrameworkCore.Sqlite** (8.0.11) - SQLite provider
- **Microsoft.EntityFrameworkCore.Tools** (10.0.1) - EF Core tools
- **Microsoft.EntityFrameworkCore.Design** (8.0.11) - Design-time tools
- **CommunityToolkit.Mvvm** (8.4.0) - MVVM helpers
- **System.Drawing.Common** (10.0.1) - Printing support
- **Moq** (4.20.72) - Mocking framework for tests

## Development

### Building

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Creating a Migration

```bash
cd src/BillPaymentManager
dotnet ef migrations add MigrationName
```

### Updating Database

The database is automatically updated on application startup. Manual update:

```bash
cd src/BillPaymentManager
dotnet ef database update
```

## Troubleshooting

### Application won't start
- Ensure .NET 8.0 SDK is installed: `dotnet --version`
- Check that all NuGet packages are restored: `dotnet restore`

### Database errors
- Delete the database file and restart the application to recreate it
- Location: `%LOCALAPPDATA%\BillPaymentManager\payments.db`

### SMS parsing not working
- Ensure the SMS text matches the expected format
- Check that the SMS contains keywords like "bKash", "Nagad", or "Rocket"
- Transaction ID and amount are required fields

## License

This project is for educational and internal use.

## Support

For questions or issues, please contact your system administrator.
