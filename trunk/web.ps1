
$web = New-Object -TypeName System.Net.WebClient
$web.UseDefaultCredentials = $true

$uri = 'https://farnet.googlecode.com/svn/trunk/PowerShellFar/Bench/Update-FarPackage.ps1'
Write-Host "Importing $uri"
Set-Content Function:\Update-FarPackage $web.DownloadString($uri)
Write-Host "Use Update-FarPackage in order to install or update Far Manager packages."
