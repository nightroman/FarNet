<#
.Synopsis
	Sets and checks the test environment.
#>

# set modes assumed by tests
$panel1 = $Far.Panel
$panel2 = $Far.Panel2
$panel1.SortMode = $panel2.SortMode = 'Name'

# check what cannot be set and others
[FarNet.Works.Test]::AssertNormalState()

# clear
Clear-Session
