# deploy.ps1
# Telegram bot deployment script

# Make sure PowerShell correctly outputs UTF-8 (for emoji)
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$ErrorActionPreference = "SilentlyContinue"

# 1. Remove old publish folder
Remove-Item -Recurse -Force .\publish -ErrorAction SilentlyContinue
Write-Host "[INFO] Publish folder cleaned"

# 2. Publish project
dotnet publish -c Release -o .\publish --no-self-contained
Write-Host "[INFO] Project built and published"

# 3. Copy files to the server
Write-Host "[INFO] Uploading files to the server..."
scp -r ./publish/* root@37.128.205.127:/root/aspnetlearning/app

# 4. Connect to server and restart bot
Write-Host "[INFO] Restarting bot on the server..."
ssh root@37.128.205.127 "/root/aspnetlearning/update_bot.sh"