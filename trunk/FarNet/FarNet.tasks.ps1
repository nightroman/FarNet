
task Clean {
	Remove-Item bin, obj -Recurse -Force -ErrorAction 0
}

task Install -Inputs bin\$Configuration\$Assembly -Outputs { "$FarHome\FarNet\$Assembly" } {process{
	Copy-Item -LiteralPath $_ $$
}}

task Uninstall {
	Remove-Item -LiteralPath "$FarHome\FarNet\$Assembly" -ErrorAction 0
}
