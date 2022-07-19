<#
.Synopsis
	Shows Colorer scheme graph as DGML.

.Description
	For the HRC file, the script generates its scheme graph as DGML. Then the
	result file is opened by the associated program, normally Visual Studio.

	For viewing in Visual Studio ensure:
	- Individual components \ Code tools \ DGML editor

.Parameter File
		Specifies the HRC file path.
.Parameter Output
		Specifies the output file path.
		Default: "$env:TEMP\Colorer-<name>-<hash>.dgml".
.Parameter NoShow
		Tells not to show the graph after creation.
#>

param(
	[Parameter(Position=0, Mandatory=1)]
	[string]$File,
	[Parameter(Position=1)]
	[string]$Output,
	[switch]$NoShow
)

$ErrorActionPreference = 1
trap {$PSCmdlet.ThrowTerminatingError($_)}

$File = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($File)
if ($Output) {
	$Output = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($Output)
}
else {
	$Output = '{0}\Colorer-{1}-{2}.dgml' -f @(
		$env:TEMP
		[IO.Path]::GetFileNameWithoutExtension($File)
		[IO.Path]::GetFileName([IO.Path]::GetDirectoryName($File))
	)
}

# get schemes
$schemeNodes = Select-Xml //x:scheme -Path $File -Namespace @{x="http://colorer.sf.net/2003/hrc"}

# XML document
$xml = [xml]@'
<?xml version="1.0" encoding="utf-8"?>
<DirectedGraph GraphDirection="TopToBottom" Layout="Sugiyama">
	<Categories>
		<Category Id="inherit" Stroke="Gray"/>
		<Category Id="block" Stroke="Blue" StrokeDashArray="3 3" />
	</Categories>
	<Properties>
		<Property Id="BlockStart" DataType="System.String" />
		<Property Id="BlockEnd" DataType="System.String" />
	</Properties>
</DirectedGraph>
'@
$doc = $xml.DocumentElement
$nodes = $doc.AppendChild($xml.CreateElement('Nodes'))
$links = $doc.AppendChild($xml.CreateElement('Links'))

# generate
foreach($schemeNode in $schemeNodes) {
	$schemeNode = $schemeNode.Node
	$name = $schemeNode.name

	$node = $nodes.AppendChild($xml.CreateElement('Node'))
	$node.SetAttribute('Id', $name)
	$node.SetAttribute('Category', 'scheme')

	$num = 0
	foreach($childNode in $schemeNode.ChildNodes) {
		if ($childNode.LocalName -ceq 'inherit') {
			++$num
			$node.SetAttribute(('x{0,2:D2}' -f $num), "inherit $($childNode.scheme)")

			$link = $links.AppendChild($xml.CreateElement('Link'))
			$link.SetAttribute('Source', $name)
			$link.SetAttribute('Target', $childNode.scheme)
			$link.SetAttribute('Category', 'inherit')
			$link.SetAttribute('Label', $num)
		}
		elseif ($childNode.LocalName -ceq 'block') {
			++$num
			$node.SetAttribute(('x{0,2:D2}' -f $num), "block $($childNode.scheme)")

			$link = $links.AppendChild($xml.CreateElement('Link'))
			$link.SetAttribute('Source', $name)
			$link.SetAttribute('Target', $childNode.scheme)
			$link.SetAttribute('Category', 'block')
			$link.SetAttribute('Label', $num)
			$link.SetAttribute('BlockStart', $childNode.GetAttribute('start'))
			$link.SetAttribute('BlockEnd', $childNode.GetAttribute('end'))
		}
		elseif ($childNode.LocalName -ceq 'regexp') {
			++$num
			$node.SetAttribute(('x{0,2:D2}' -f $num), "regexp $($childNode.GetAttribute('match'))")
		}
	}
}

# finish, save, open
$doc.SetAttribute('xmlns', 'http://schemas.microsoft.com/vs/2009/dgml')
$xml.Save($Output)
if (!$NoShow) {
	Invoke-Item $Output
}

