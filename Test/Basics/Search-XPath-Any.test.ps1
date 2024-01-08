
Set-StrictMode -Version 3

class Any1 {
	[string]$String = 'bar'
	[bool]$BoolTrue = $true
	[bool]$BoolFalse = $false
	[datetime]$Old = '2000-01-01'
	[datetime]$New = '2024-01-04 01:01:01.12345'
	[int]$Int = 42
	[Version]$Version = '1.2.3'
	[cultureinfo]$Culture = 'en-GB'
	[Version]$NullProperty = $null
	[Any2]$Any = [Any2]::new()
	[object]$ValueTuple = [System.ValueTuple]::Create('p1', 42)
	[object[]]$Collection = @(42, 'bar', $null)
	[object]$Dictionary = [ordered]@{Int = 42; String = 'bar'; NullValue = $null}
}

class Any2 {
	[string]$Name = 'May'
	[int]$Age = 33
}

$ExpectedXml = @'
<item type="Any1">
  <data name="String" type="String">bar</data>
  <data name="BoolTrue" type="Boolean">1</data>
  <data name="BoolFalse" type="Boolean">0</data>
  <data name="Old" type="DateTime">2000-01-01</data>
  <data name="New" type="DateTime">2024-01-04 01:01:01</data>
  <data name="Int" type="Int32">42</data>
  <data name="Version" type="Version">1.2.3</data>
  <data name="Culture" type="CultureInfo">en-GB</data>
  <data name="NullProperty" type="Version"></data>
  <item name="Any" type="Any2">
    <data name="Name" type="String">May</data>
    <data name="Age" type="Int32">33</data>
  </item>
  <item name="ValueTuple" type="ValueTuple`2">
    <data name="Item1" type="String">p1</data>
    <data name="Item2" type="Int32">42</data>
  </item>
  <list name="Collection" type="Object[]">
    <data type="Int32">42</data>
    <data type="String">bar</data>
    <data type="Object"></data>
  </list>
  <list name="Dictionary" type="OrderedDictionary">
    <data name="Int" type="Int32">42</data>
    <data name="String" type="String">bar</data>
    <data name="NullValue" type="Object"></data>
  </list>
</item>
'@

$RootObject = [Any1]::new()

task tree_xml {
	$nav = [FarNet.Tools.XPathObjectNavigator]::new($RootObject, -1)
	($xml = $nav.InnerXml)
	equals $xml $ExpectedXml
}

task value_predicate {
	$nav = [FarNet.Tools.XPathObjectNavigator]::new($RootObject, -1)
	$r = $nav.Select('//data[.=1]').ForEach{$_.OuterXml}
	equals "$r" '<data name="BoolTrue" type="Boolean">1</data>'
}

# how to use variable and underlying object
task variable {
	$nav = [FarNet.Tools.XPathObjectNavigator]::new($RootObject, -1)
	$xp = $nav.Compile('//data[.=$version]', @{version = '1.2.3'})
	$r = $nav.SelectSingleNode($xp)
	equals $r.UnderlyingObject ([version]'1.2.3')
}

# function `equals`, helper with ignore case
task equals {
	$nav = [FarNet.Tools.XPathObjectNavigator]::new($RootObject, -1)
	$xp = $nav.Compile('//data[equals(., "BAR")]')
	$r = @($nav.Select($xp))
	equals $r.Count 3
}

# function `regex`, with inline options
task regex {
	$nav = [FarNet.Tools.XPathObjectNavigator]::new($RootObject, -1)
	$xp = $nav.Compile('//data[is-match(., "(?i)^BA")]')
	$r = @($nav.Select($xp))
	equals $r.Count 3
}

# function `compare` strings, less < 0, equal = 0, greater > 0
task compare {
	$nav = [FarNet.Tools.XPathObjectNavigator]::new($RootObject, -1)
	$xp = $nav.Compile('//data[@type="DateTime" and compare(., "2024-01-01") < 0]')
	$r = @($nav.Select($xp))
	equals $r.Count 1
}

# when depth, values are returned but deeper elements are not
task depth {
	$nav = [FarNet.Tools.XPathObjectNavigator]::new($RootObject, 1)
	($xml = $nav.InnerXml)
	assert ($xml.Contains('<data name="String" type="String">bar</data>'))
	assert ($xml.Contains('<item name="Any" type="Any2" />'))
	assert ($xml.Contains('<list name="Collection" type="Object[]" />'))
	assert ($xml.Contains('<list name="Dictionary" type="OrderedDictionary" />'))
}

# when depth is unlimited, loops should be avoided
task avoid_loops {
	$root = @{}
	$root.loop = $root

	# unlimited depth
	$nav = [FarNet.Tools.XPathObjectNavigator]::new($root, -1)
	($xml = $nav.InnerXml)
	assert ($xml.Contains('  <list name="loop" type="Hashtable" />'))

	# limited depth
	$nav = [FarNet.Tools.XPathObjectNavigator]::new($root, 2)
	($xml = $nav.InnerXml)
	assert ($xml.Contains('  <list name="loop" type="Hashtable">'))
	assert ($xml.Contains('    <list name="loop" type="Hashtable" />'))
}
