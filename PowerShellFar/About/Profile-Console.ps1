<#
.Synopsis
	Sample commands console profile.

.Description
	Location: %FARPROFILE%\FarNet\PowerShellFar\Profile-Console.ps1
#>

function prompt {
	$width = $Far.UI.WindowSize.X

	$line = "┌─`"$($PWD)`"".PadRight($width)
	if ($line.Length -gt $width) {
		[int]$half = $width / 2
		$line = $line.Substring(0, $half) + '*' + $line.Substring($line.Length - $width + $half + 1)
	}

	$Far.UI.WriteLine($line)
	"└─($([datetime]::Now.ToString('HH:MM:ss')))─> "
}
