
Set-StrictMode -Version 2
$outpath = 'C:\TEMP\Files.xslt.out.xml'

$files = $Far.Panel.ShownFiles
$nav = [FarNet.Tools.XPathObjectNavigator]$files
[io.File]::WriteAllText($outpath, $nav.InnerXml)

Invoke-Item $outpath
