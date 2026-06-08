param(
    [string]$ConnectionString = 'Server=localhost,1433;Database=ITStockM;User Id=sa;Password=AsteelFlash@2026;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=True'
)

$rootDir = Split-Path -Parent (Split-Path -Path $MyInvocation.MyCommand.Path)
Set-Location $rootDir

Write-Host 'Starting local SQL Server (Docker)...'
& (Join-Path $rootDir 'scripts\start_mssql_docker.ps1') -ContainerName 'itstockm-mssql' -SaPassword 'AsteelFlash@2026'

Write-Host 'Setting connection string environment variable for this session...'
$env:ConnectionStrings__ITStockManagmentConnection = $ConnectionString
Write-Host 'Connection string set.'

function Ensure-DotNetEf {
    $dotnetEf = Get-Command dotnet-ef -ErrorAction SilentlyContinue
    if (-not $dotnetEf) {
        Write-Host 'dotnet-ef not found. Installing global tool...'
        dotnet tool install --global dotnet-ef
        if ($LASTEXITCODE -ne 0) {
            Write-Error 'Failed to install dotnet-ef. Please install it manually and rerun this script.'
            exit $LASTEXITCODE
        }
        $env:PATH = "$env:PATH;$HOME\.dotnet\tools"
    }
}

Ensure-DotNetEf

$migrationsDir = Join-Path $rootDir 'src/ITStockM.Infrastructure/Migrations'
$project = 'src/ITStockM.Infrastructure/ITStockM.Infrastructure.csproj'
$startupProject = 'src/ITStockM.WebApi/ITStockM.WebApi.csproj'

if (Test-Path $migrationsDir -PathType Container -and (Get-ChildItem $migrationsDir -File | Measure-Object).Count -gt 0) {
    Write-Host 'Existing migrations found — applying to database...'
    dotnet ef database update --project $project --startup-project $startupProject
} else {
    Write-Host 'No migrations found — creating "InitialCreate" migration and applying...'
    dotnet ef migrations add InitialCreate --project $project --startup-project $startupProject
    dotnet ef database update --project $project --startup-project $startupProject
}

Write-Host 'Building solution...'
dotnet build ITStockM.sln

Write-Host 'Running WebApi project...'
dotnet run --project $startupProject
