# PowerShell script to clean database using Entity Framework
# This will drop the entire database and remove all tables and migration history

Write-Host "WARNING: This will DELETE ALL DATA from the database!" -ForegroundColor Red
Write-Host "Database: EcommerceAdmin" -ForegroundColor Yellow
$confirmation = Read-Host "Are you sure you want to continue? (yes/no)"

if ($confirmation -eq "yes") {
    Write-Host "Dropping database..." -ForegroundColor Yellow
    
    # Drop the database
    dotnet ef database drop --project Infrastructure --startup-project ECommerce.Server --force
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Database dropped successfully!" -ForegroundColor Green
        Write-Host "You can now create a fresh migration with:" -ForegroundColor Cyan
        Write-Host "dotnet ef migrations add InitialIdentityAndPermissions --project Infrastructure --startup-project ECommerce.Server" -ForegroundColor Cyan
    } else {
        Write-Host "Error dropping database. You may need to manually clean it." -ForegroundColor Red
    }
} else {
    Write-Host "Operation cancelled." -ForegroundColor Yellow
}

