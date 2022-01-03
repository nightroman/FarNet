
# kill test file
Remove-Item c:/tmp/tmp.fsx*
job {
	Open-FarEditor c:/tmp/tmp.fsx -DisableHistory
}
macro "print'Micros' -- type text"
macro "Keys'Tab' -- complete"
job {
	#! fixed
	Assert-Far $Far.Editor.GetText() -eq 'Microsoft'
}
macro "Keys'Esc n' -- exit"
job {
	Assert-Far (![IO.File]::Exists('c:/tmp/tmp.fsx'))
}
