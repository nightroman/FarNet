<#
	*** Run as script, not ib-task ***

	The change
		zg 15.08.2018 00:00:47 +0300 - build 5256
		1. все вызовы плагинов защищены глобальной критической секцией.
	causes hangs in PowerShellFar 5.2.5.
	PowerShellFar 5.2.6 deals with this.
	Far 3.0.5260 reverts 5256 but we keep our change for other similar cases in Far.
	Thus, we need a test covering future changes like 5256.
	That is why we call Far using a new runspace.
	The test will hang if 5256 comes back.
#>

### part 1: warming up in order to trigger the editor profile
# If we do not then in a clean session the profile is invoked
# in the editor event handler and this fails "nested pipeline"

$editor = $Far.CreateEditor()
$editor.Open()

Assert-Far -Editor
$__.Close()
Assert-Far -Panels

### part 2: actual test
# We normally should not call Far API from another runspace.
# But it should work, at least with the part 1 preparation.

$ps = [PowerShell]::Create().AddScript({
	$api = [FarNet.Far]::Api
	$editor = $api.CreateEditor()
	$editor.Open()
})
$ps.Invoke()
$ps.Dispose()

Assert-Far -Editor
$__.Close()
Assert-Far -Panels
