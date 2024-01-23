# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/P5R.CustomCursor/*" -Force -Recurse
dotnet publish "./P5R.CustomCursor.csproj" -c Release -o "$env:RELOADEDIIMODS/P5R.CustomCursor" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location