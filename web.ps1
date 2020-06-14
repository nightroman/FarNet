
$web = New-Object -TypeName System.Net.WebClient
$web.UseDefaultCredentials = $true

$uri = 'https://raw.githubusercontent.com/nightroman/FarNet/master/PowerShellFar/Modules/FarPackage/FarPackage.psm1'
Write-Host "Importing $uri"
Invoke-Expression $web.DownloadString($uri)
Write-Host @'
Imported commands (for more details use: Get-Help <command>):
Install-FarPackage   - installs or updates one package
Update-FarPackage    - updates all installed packages
Uninstall-FarPackage - removes the specified package
'@
