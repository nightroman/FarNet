
<#
.Synopsis
	Shows Colorer scheme graph using Graphviz.
	Author: Roman Kuzmin, 2016-02-05

.Description
	Requires:
	- Graphviz: http://graphviz.org/
	- Graphviz\bin as the environment variable Graphviz or in the path.

.Parameter File
		Specifies the HRC file path.
.Parameter Output
		The output file path and format specified by extension. For available
		formats simply use unlikely supported one and check the error message.
		The default is "$env:TEMP\Graphviz.pdf".
.Parameter Code
		Custom DOT code added to the graph definition, see Graphviz manuals.
		The default 'graph [rankdir=TB]' tells edges to go from top to bottom.
.Parameter NoShow
		Tells to not show the graph after creation.
#>

param(
	[Parameter(Position=0, Mandatory=1)]
	[string]$File,
	[Parameter(Position=1)]
	[string]$Output = "$env:TEMP\Graphviz.pdf",
	[string]$Code = 'graph [rankdir=TB]',
	[switch]$NoShow
)

trap {$PSCmdlet.ThrowTerminatingError($_)}

# resolve dot.exe
$dot = if ($env:Graphviz) {"$env:Graphviz\dot.exe"} else {'dot.exe'}
$dot = Get-Command $dot -CommandType Application -ErrorAction 0
if (!$dot) {throw 'Cannot find dot.exe'}

# output
$type = [System.IO.Path]::GetExtension($Output)
if (!$type) {throw "Output file name should have an extension."}
$type = $type.Substring(1).ToLower()
$Output = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($Output)

# get schemes
$xml = Select-Xml //x:scheme -Path $File -Namespace @{x="http://colorer.sf.net/2003/hrc"}

# DOT code
$text = @(
	'digraph scheme {'
	$Code
	foreach($node in $xml) {
		$it = $node.Node
		$name = $it.name
		'"{0}"' -f $name

		$num = 0
		foreach($node in $it.ChildNodes) {
			++$num
			if ($node.LocalName -eq 'inherit') {
				'"{0}" -> "{1}" [{2}]' -f $name, $node.scheme, " label=$num "
			}
			elseif ($node.LocalName -eq 'block') {
				'"{0}" -> "{1}" [{2}]' -f $name, $node.scheme, " style=dotted label=$num "
			}
		}
	}
	'}'
)

#! temp file UTF8 no BOM
$temp = "$env:TEMP\Graphviz.dot"
[System.IO.File]::WriteAllLines($temp, $text)

# make
& $dot "-T$type" -o $Output $temp
if ($LastExitCode) {return}

# show
if ($NoShow) {return}
Invoke-Item $Output
