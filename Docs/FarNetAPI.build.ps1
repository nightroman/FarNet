
param(
	$Platform = (property Platform x64)
)
$FarHome = "C:\Bin\Far\$Platform"

task build {
	Set-Alias MSBuild (Resolve-MSBuild)
	exec { MSBuild FarNetAPI.shfbproj /p:Configuration=Release }
},
test

task install {
	assert (Test-Path Help\FarNetAPI.chm) "Please, invoke Build."
	remove $FarHome\FarNet\FarNetAPI.*
	Copy-Item Help\FarNetAPI.chm $FarHome\FarNet
}

task clean {
	remove Help, obj, *.shfbproj_*
}

task test {
     $r = Select-Xml -Path Help\LastBuild.log -XPath 'shfbBuild/buildStep[@step="BuildReferenceTopics"]'
     $lines = $r.Node.'#text' -split '[\r\n]+'
     foreach($line in $lines) {
          if ($line -match "Warn: ResolveReferenceLinksComponent2: \[(.*?)\] Unknown reference link target '(.*?)'") {
               $n1 = $matches[1]
               $n2 = $matches[2]
               if ($n2 -like 'T:System.Management.*') {}
               else {
                    Write-Warning ('Reference: {0} -> {1}' -f $n1, $n2)
               }
          }
          elseif ($line -match "Warn: (.*)") {
               Write-Warning ($matches[1])
          }
     }
}

task run {
	Invoke-Item $FarHome\FarNet\FarNetAPI.chm
}

task . build, install, run
