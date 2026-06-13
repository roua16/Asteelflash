param(
    [string]$ContainerName = 'itstockm-mssql',
    [string]$SaPassword = 'AsteelFlash@2026'
)

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Error 'Docker is not installed or not available in PATH. Install Docker Desktop and try again.'
    exit 1
}

Write-Host "Pulling SQL Server image..."
$pullResult = docker pull mcr.microsoft.com/mssql/server:2019-latest
if ($LASTEXITCODE -ne 0) {
    Write-Error 'Failed to pull the SQL Server image.'
    exit $LASTEXITCODE
}

$containerExists = docker ps -a --format '{{.Names}}' | Select-String -Pattern "^$ContainerName$"
if ($containerExists) {
    Write-Host "Container $ContainerName already exists. Starting it..."
    docker start $ContainerName | Out-Null
} else {
    Write-Host "Creating and running container $ContainerName bound to localhost:1433"
    docker run -e 'ACCEPT_EULA=Y' -e "SA_PASSWORD=$SaPassword" -p 1433:1433 --name $ContainerName -d mcr.microsoft.com/mssql/server:2019-latest | Out-Null
}

Write-Host 'Waiting for SQL Server to be ready...'
for ($i = 0; $i -lt 30; $i++) {
    $logs = docker logs $ContainerName 2>&1 | Select-String 'SQL Server is now ready for client connections'
    if ($logs) {
        Write-Host 'SQL Server is ready.'
        exit 0
    }
    Start-Sleep -Seconds 2
}

Write-Error "Timed out waiting for SQL Server to be ready. Check container logs with: docker logs $ContainerName"
exit 1
