
Set-StrictMode -Version 3

### Get the whole tree as XML

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

$expected = @'
<Item Type="Any1">
  <Item Name="String" Type="String">bar</Item>
  <Item Name="BoolTrue" Type="Boolean">1</Item>
  <Item Name="BoolFalse" Type="Boolean">0</Item>
  <Item Name="Old" Type="DateTime">2000-01-01</Item>
  <Item Name="New" Type="DateTime">2024-01-04 01:01:01</Item>
  <Item Name="Int" Type="Int32">42</Item>
  <Item Name="Version" Type="Version">1.2.3</Item>
  <Item Name="Culture" Type="CultureInfo">en-GB</Item>
  <Item Name="NullProperty" Type="Version"></Item>
  <Item Name="Any" Type="Any2">
    <Item Name="Name" Type="String">May</Item>
    <Item Name="Age" Type="Int32">33</Item>
  </Item>
  <Item Name="ValueTuple" Type="ValueTuple`2">
    <Item Name="Item1" Type="String">p1</Item>
    <Item Name="Item2" Type="Int32">42</Item>
  </Item>
  <List Name="Collection" Type="Object[]">
    <Item Type="Int32">42</Item>
    <Item Type="String">bar</Item>
    <Item Type="Object"></Item>
  </List>
  <List Name="Dictionary" Type="OrderedDictionary">
    <Item Name="Int" Type="Int32">42</Item>
    <Item Name="String" Type="String">bar</Item>
    <Item Name="NullValue" Type="Object"></Item>
  </List>
</Item>
'@

$root = [Any1]::new()
$nav = [FarNet.Tools.XPathObjectNavigator]::new($root, -1)
($xml = $nav.InnerXml)
Assert-Far $xml -eq $expected

### How to query a value (`.` is the same as `text()`)

$r = @($nav.Select('//Item[.=1]').ForEach{$_.OuterXml})
Assert-Far $r.Count -eq 1
Assert-Far "$r" -eq '<Item Name="BoolTrue" Type="Boolean">1</Item>'

### How to use variables, functions, and underlying objects

# use variable and result underlying object
$xp = $nav.Compile('//Item[.=$version]', @{version = '1.2.3'})
$r = $nav.SelectSingleNode($xp)
Assert-Far $r.UnderlyingObject -eq ([version]'1.2.3')

# equals (helper with ignore case)
$xp = $nav.Compile('//Item[equals(., "BAR")]')
$r = @($nav.Select($xp))
Assert-Far $r.Count -eq 3

# regex (use inline options)
$xp = $nav.Compile('//Item[is-match(., "(?i)^BA")]')
$r = @($nav.Select($xp))
Assert-Far $r.Count -eq 3

# compare strings, less < 0, equal = 0, greater > 0
$xp = $nav.Compile('//Item[@Type="DateTime" and compare(., "2024-01-01") < 0]')
$r = @($nav.Select($xp))
Assert-Far $r.Count -eq 1

### Limit tree depth
# values are returned but deeper elements are not

$nav = [FarNet.Tools.XPathObjectNavigator]::new($root, 1)
$xml = $nav.InnerXml
Assert-Far ($xml.Contains('<Item Name="String" Type="String">bar</Item>'))
Assert-Far ($xml.Contains('<Item Name="Any" Type="Any2"></Item>'))
Assert-Far ($xml.Contains('<List Name="Collection" Type="Object[]"></List>'))
Assert-Far ($xml.Contains('<List Name="Dictionary" Type="OrderedDictionary"></List>'))
