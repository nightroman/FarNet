<#
.Synopsis
	Test module panels closing and garbage disposal.

.Description
	Correct order: Child-Closed.Child-Disposed.Parent-Closed.Parent-Disposed.
	Handlers may use host resources and children may use parents resources.
	Do not break parent-child links, not disposable data are fine to use.
#>

Add-Type @'
using System;
using System.Management.Automation;
public class TestPanelClosed : IDisposable
{
	ScriptBlock _script;
	public TestPanelClosed(ScriptBlock script) { _script = script; }
	public void Dispose() { _script.InvokeReturnAsIs(); }
}
'@

# log
$Data.Output = ''

$parent = { job {
	# parent panel
	(New-Object PowerShellFar.PowerExplorer '2660b316-e947-4d26-b812-997dfb8a448d' -Property @{
		AsCreatePanel = {
			param($0)
			$Panel = [FarNet.Panel]$0
			$Panel.Title = 'parent panel'
			$Panel.add_Closed({ $Data.Output += "Parent-Closed." })
			$Panel.Garbage.Add(([TestPanelClosed]{ $Data.Output += "Parent-Disposed." }))
			$Panel
		}
	}).CreatePanel().Open()
}}

$child = { job {
	# child panel
	(New-Object PowerShellFar.PowerExplorer 'a26794ae-1ae6-4480-ab25-b0f5d402262f' -Property @{
		AsCreatePanel = {
			param($0)
			$Panel = [FarNet.Panel]$0
			$Panel.Title = 'child panel'
			$Panel.add_Closed({ $Data.Output += "Child-Closed." })
			$Panel.Garbage.Add(([TestPanelClosed]{ $Data.Output += "Child-Disposed." }))
			$Panel
		}
	}).CreatePanel().OpenChild($Far.Panel)
}}

### open parent
& $parent

### open child, close child
& $child
job {
	Assert-Far -Plugin
	Assert-Far ($Far.Panel.Parent -ne $null)
	$Far.Panel.CloseChild()
}
job {
	Assert-Far -Plugin
	Assert-Far @(
		$Far.Panel.Title -eq 'parent panel'
		$Data.Output -eq 'Child-Closed.Child-Disposed.'
	)
	$Data.Output = ''
}

### open child, close all
& $child
job {
	Assert-Far -Plugin
	Assert-Far ($Far.Panel.Parent -ne $null)
	$Far.Panel.Close()
}
job {
	Assert-Far -Native
	Assert-Far $Data.Output -eq 'Child-Closed.Child-Disposed.Parent-Closed.Parent-Disposed.'
	$Data.Output = ''
}
