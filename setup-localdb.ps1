# LocalDB Setup and Diagnostic Script
Write-Host "=== LocalDB Diagnostic Tool ===" -ForegroundColor Cyan

# Check if LocalDB is installed
Write-Host "`nChecking if LocalDB is installed..." -ForegroundColor Yellow
try {
    $instances = sqllocaldb info
    Write-Host "LocalDB is installed!" -ForegroundColor Green
    Write-Host "Available instances:" -ForegroundColor White
    $instances | ForEach-Object { Write-Host "  - $_" }
} catch {
    Write-Host "LocalDB is NOT installed or not in PATH" -ForegroundColor Red
    Write-Host "Install it from: https://go.microsoft.com/fwlink/?linkid=866662" -ForegroundColor Yellow
    exit
}

# Check if mssqllocaldb instance exists
Write-Host "`nChecking for 'mssqllocaldb' instance..." -ForegroundColor Yellow
$instanceExists = $instances -contains "mssqllocaldb"

if (-not $instanceExists) {
    Write-Host "'mssqllocaldb' instance not found. Creating it..." -ForegroundColor Yellow
    sqllocaldb create mssqllocaldb
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Instance created successfully!" -ForegroundColor Green
    } else {
        Write-Host "Failed to create instance" -ForegroundColor Red
        exit
    }
}

# Check instance status
Write-Host "`nChecking instance status..." -ForegroundColor Yellow
$instanceInfo = sqllocaldb info mssqllocaldb
Write-Host $instanceInfo

# Start the instance if not running
Write-Host "`nStarting 'mssqllocaldb' instance..." -ForegroundColor Yellow
sqllocaldb start mssqllocaldb

if ($LASTEXITCODE -eq 0) {
    Write-Host "Instance started successfully!" -ForegroundColor Green
} else {
    Write-Host "Instance may already be running or failed to start" -ForegroundColor Yellow
}

# Display detailed instance information
Write-Host "`nInstance Details:" -ForegroundColor Cyan
sqllocaldb info mssqllocaldb -v

Write-Host "`n=== Setup Complete ===" -ForegroundColor Green
Write-Host "You can now run your Blazor application!" -ForegroundColor White
