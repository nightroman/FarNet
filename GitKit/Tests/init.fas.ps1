
### init path

job {
	if (Test-Path c:\tmp\z) {
		Remove-Item c:\tmp\z -Force -Recurse
	}

	$Far.InvokeCommand("gk:init path=c:\tmp\z")
}

job {
	Assert-Far .git -eq (Get-Item c:\tmp\z\* -Force).Name
}

### init path isBare

job {
	if (Test-Path c:\tmp\z) {
		Remove-Item c:\tmp\z -Force -Recurse
	}

	$Far.InvokeCommand("gk:init path=c:\tmp\z; isBare=true")
}

job {
	Assert-Far hooks/info/objects/refs/config/description/HEAD -eq ((Get-Item c:\tmp\z\*).ForEach{$_.Name} -join '/')
}
