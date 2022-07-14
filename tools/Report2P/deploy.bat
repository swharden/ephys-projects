rmdir /S /Q X:\Software\Report2P
dotnet publish --configuration Release --self-contained true --runtime win-x86
robocopy bin\Release\net6.0-windows\win-x86 X:\Software\Report2P /E /NJH /NFL /NDL
pause