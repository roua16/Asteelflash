<#
Simple SMTP connectivity + send test (PowerShell)
Usage:
  pwsh ./scripts/send-email-test.ps1
  pwsh ./scripts/send-email-test.ps1 -SmtpServer smtp.gmail.com -Port 587 -From you@example.com -Password "app-password" -To admin@example.com
Notes:
- For Gmail use an App Password or enable the appropriate account settings (Gmail blocks plain username/password unless app password is used).
- This script performs a TCP connect test first, then uses System.Net.Mail.SmtpClient to send a small test email (STARTTLS on port 587).
#>
param(
    [string]$SmtpServer = $env:SMTP_SERVER,
    [int]$Port = 587,
    [string]$From = $env:SMTP_FROM_EMAIL,
    [string]$Password = $env:SMTP_PASSWORD,
    [string]$To = $env:SMTP_ADMIN_EMAIL,
    [switch]$NoSsl,
    [string]$Subject = "Test email from ITStockM",
    [string]$Body = "This is a test email sent on $(Get-Date -Format o) by send-email-test.ps1"
)
if ($env:SMTP_PORT) { $Port = [int]$env:SMTP_PORT }

function Test-TcpPort {
    param($TargetHost, $Port, $TimeoutSec = 5)
    try {
        $tcp = New-Object System.Net.Sockets.TcpClient
        $async = $tcp.BeginConnect($TargetHost, $Port, $null, $null)
        $success = $async.AsyncWaitHandle.WaitOne([TimeSpan]::FromSeconds($TimeoutSec))
        if (-not $success) { $tcp.Close(); return $false }
        $tcp.EndConnect($async)
        $tcp.Close()
        return $true
    }
    catch {
        return $false
    }
}

if (-not $SmtpServer) { Write-Error "SMTP server not provided (env SMTP_SERVER or -SmtpServer)"; exit 2 }
if (-not $From -or -not $To) { Write-Error "Missing From/To. Set SMTP_FROM_EMAIL and SMTP_ADMIN_EMAIL in env or pass as params."; exit 2 }

Write-Host "Testing TCP connection to ${SmtpServer}:${Port}..."
if (-not (Test-TcpPort -TargetHost $SmtpServer -Port $Port -TimeoutSec 6)) {
    exit 3
}
Write-Host "TCP connect OK. Attempting SMTP send..."

try {
    $mail = New-Object System.Net.Mail.MailMessage($From, $To, $Subject, $Body)
    $smtp = New-Object System.Net.Mail.SmtpClient($SmtpServer, $Port)
    $smtp.EnableSsl = (-not $NoSsl)
    $smtp.Timeout = 15000
    $smtp.DeliveryMethod = [System.Net.Mail.SmtpDeliveryMethod]::Network
    if ($Password) {
        $smtp.Credentials = New-Object System.Net.NetworkCredential($From, $Password)
    } else {
        $smtp.Credentials = $null
        $smtp.UseDefaultCredentials = $false
    }
    Write-Host "Sending email from ${From} to ${To} via ${SmtpServer}:${Port} ..."
    $smtp.Send($mail)
    Write-Host "Email sent successfully ✅"
}
catch {
    Write-Error "Sending email failed: $($_.Exception.Message)"
    if ($_.Exception.InnerException) { Write-Error "Inner: $($_.Exception.InnerException.Message)" }
    exit 4
}
