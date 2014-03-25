
$web = New-Object -TypeName System.Net.WebClient
$web.UseDefaultCredentials = $true

$uri = 'https://farnet.googlecode.com/svn/trunk/PowerShellFar/Bench/Update-FarPackage.ps1'
Write-Host "Import $uri"
Set-Content Function:\Update-FarPackage $web.DownloadString($uri)
