
Set-StrictMode -Version 2
$outpath = 'C:\TEMP\Files.xslt.out.xml'

Add-Type -Path $env:FARHOME\FarNet\FarNet.Tools.dll

$files = $Far.Panel.ShownFiles
$nav = [FarNet.Tools.XPathObjectNavigator]$files
[io.File]::WriteAllText($outpath, $nav.InnerXml)

Invoke-Item $outpath
