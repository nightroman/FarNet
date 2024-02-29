
### init path

job {
	if (Test-Path c:\temp\z) {
		Remove-Item c:\temp\z -Force -Recurse
	}

	$Far.InvokeCommand("gk:init path=c:\temp\z")
}

job {
	Assert-Far .git -eq (Get-Item c:\temp\z\* -Force).Name
}

### init path isBare

job {
	if (Test-Path c:\temp\z) {
		Remove-Item c:\temp\z -Force -Recurse
	}

	$Far.InvokeCommand("gk:init path=c:\temp\z; isBare=true")
}

job {
	Assert-Far hooks/info/objects/refs/config/description/HEAD -eq ((Get-Item c:\temp\z\*).ForEach{$_.Name} -join '/')
}
