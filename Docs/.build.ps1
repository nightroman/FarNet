
param(
	$Platform = (property Platform x64)
)
$FarHome = "C:\Bin\Far\$Platform"

task Build {
	Set-Alias MSBuild (Resolve-MSBuild)
	exec { MSBuild FarNetAPI.shfbproj /p:Configuration=Release }
},
Test

task Install {
	assert (Test-Path Help\FarNetAPI.chm) "Please, invoke Build."
	Remove-Item $FarHome\FarNet\FarNetAPI.*
	Copy-Item Help\FarNetAPI.chm $FarHome\FarNet
}

task Clean {
	remove Help, obj, *.shfbproj_*
}

task Test {
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
