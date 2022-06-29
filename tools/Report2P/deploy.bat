rmdir /S /Q X:\Software\Report2P
dotnet build --configuration Release
robocopy bin\Release\net6.0-windows X:\Software\Report2P /E /NJH /NFL /NDL
pause