@echo off
echo Publishing Bill Payment Manager...
dotnet clean
dotnet restore
dotnet build
dotnet publish src/BillPaymentManager/BillPaymentManager.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
echo.
echo Publish complete! 
echo.
echo Now you can:
echo 1. Open 'setup.iss' with Inno Setup Compiler.
echo 2. Click Run/Compile to generate the 'BillPaymentManager-Installer.exe'.
echo.
pause
