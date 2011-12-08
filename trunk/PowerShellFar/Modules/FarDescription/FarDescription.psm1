
<#
.Synopsis
	Updates Far descriptions of scripts by synopses.
	Author: Roman Kuzmin

.Description
	For any *.ps1 script in the directory it takes the first line of the
	synopsis, if any, and sets it as the Far description (Descript.ion).

	This is an example of how PowerShell extended type system works. The module
	FarDescription adds the property FarDescription to FileSystemInfo objects:
	FileInfo, DirectoryInfo (FileSystem provider items).
#>
function Update-FarDescriptionSynopsis
(
	[Parameter()][string]
	# Directory path where descriptions of *.ps1 scripts are updated.
	$DirectoryPath = '.'
)
{
	$DirectoryPath = Convert-Path -LiteralPath $DirectoryPath

	foreach($_ in [System.IO.Directory]::GetFiles($DirectoryPath, '*.ps1')) {
		try {
			$path = $_
			$help = Get-Help $path -ErrorAction Stop
			$file = [System.IO.FileInfo]$path
			$synopsis = $help.Synopsis.Trim()
			if ($synopsis -notlike "$($file.Name)*") {
				$description = ($synopsis -split '[\r\n]+')[0]
				if ($file.FarDescription -cne $description) {
					$file.FarDescription = $description
				}
			}
		}
		catch {
			Write-Warning "Cannot get help for '$path': $_"
		}
	}
}

Export-ModuleMember -Function *
