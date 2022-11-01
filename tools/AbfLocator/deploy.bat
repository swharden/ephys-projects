rmdir /S /Q X:\Software\AbfDB\AbfLocator
dotnet publish --configuration Release 
robocopy bin\Release\net6.0-windows X:\Software\AbfDB\AbfLocator /E /NJH /NFL /NDL
pause