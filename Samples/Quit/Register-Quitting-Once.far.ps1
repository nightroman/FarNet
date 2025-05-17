<#
.Synopsis
	How to register quitting handlers once (one possible way).

.Description
	How to use:
	- Register [F10] macro, see README.
	- Run this and see printed messages about registered handlers.
	- Run this again, see nothing this time.
	- Hit [F10] and try Yes, No, [Esc] in JOB_1 and JOB_2 dialogs.

	This script registers two different quitting handlers in order to show
	their cooperative work. Each checks for the input $_.Ignore. If it is
	true then quitting is cancelled by other handlers, UI may be avoided.
#>

# Tracing.
function global:print {$Far.UI.WriteLine($args)}

# Register JOB_1 quitting handler once.
[FarNet.User]::GetOrAdd('59d80deb-669c-43e5-ac76-88e4280d1a0d', {
	print "JOB_1 Register quitting handler..."
	$null = [FarNet.User]::RegisterQuitting({
		# Already cancelled?
		if ($_.Ignore) {
			return
		}

		# UI
		$answer = Show-FarMessage JOB_1 Quit? -Buttons YesNo

		# Yes
		if ($answer -eq 0) {
			return
		}

		# No, [Esc], etc.
		$_.Ignore = $true
	})
})

# Register JOB_2 quitting handler once.
[FarNet.User]::GetOrAdd('bf9ff329-b06a-4948-803d-6b1b1643d2b7', {
	print "JOB_2 Register quitting handler..."
	$null = [FarNet.User]::RegisterQuitting({
		# Already cancelled?
		if ($_.Ignore) {
			return
		}

		# UI
		$answer = Show-FarMessage JOB_2 Quit? -Buttons YesNo

		# Yes
		if ($answer -eq 0) {
			return
		}

		# No, [Esc], etc.
		$_.Ignore = $true
	})
})
