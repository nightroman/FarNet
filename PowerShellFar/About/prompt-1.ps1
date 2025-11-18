<#
.Synopsis
	Custom command console prompt.

.Description
	It makes the prompt like:

		┌─"C:\Bin\Far\x64"
		└─(01:11:36)─>

	F9 / Options / Command line settings / Set command line prompt format
	may be set to something "similar", e.g.

		└─[$t]─>$+$s

	HOW TO USE

	Dot-source this, either temporary in the main session or permanently in
	"Profile-Console.ps1":

		. <path>\prompt-1.ps1
#>

function prompt {
	$width = $Far.UI.WindowSize.X

	$line = "┌─`"$($PWD)`"".PadRight($width)
	if ($line.Length -gt $width) {
		[int]$half = $width / 2
		$line = $line.Substring(0, $half) + '*' + $line.Substring($line.Length - $width + $half + 1)
	}

	$Far.UI.WriteLine($line, 'DarkGray')
	"└─($([datetime]::Now.ToString('HH:MM:ss')))─> "
}
