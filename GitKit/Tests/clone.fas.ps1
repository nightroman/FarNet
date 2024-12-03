
### clone shallow

job {
	if (Test-Path c:\temp\z) {
		Remove-Item c:\temp\z -Force -Recurse
	}

	$Far.InvokeCommand("gk:clone path=c:\temp\z; depth=1; url=https://gist.github.com/95d318d6a34927f74eba.git")
}

job {
	Assert-Far test.txt/test1.txt/test2.txt/z.md/z.png -eq ((Get-Item c:\temp\z\*).ForEach{$_.Name} -join '/')
}

### clone bare

job {
	if (Test-Path c:\temp\z) {
		Remove-Item c:\temp\z -Force -Recurse
	}

	$Far.InvokeCommand("gk:clone path=c:\temp\z; isBare=true; url=https://gist.github.com/95d318d6a34927f74eba.git")
}

job {
	Assert-Far hooks/info/objects/refs/config/description/FETCH_HEAD/HEAD -eq ((Get-Item c:\temp\z\*).ForEach{$_.Name} -join '/')
}
