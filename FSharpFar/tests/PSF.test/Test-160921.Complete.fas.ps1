<#
	Complete in script
#>

job {
	# edit a missing file
	Open-FarEditor "c:\tmp\$([guid]::NewGuid()).fsx"
}

#! FarNet 9.0.45, FSF 5.1.6, ideally test without .editorconfig
# should not trigger completion
# should insert spaces
keys Tab
job {
	Assert-Far $Far.Editor.Line.Text -eq '    '
}

macro "print'Microso'; Keys'Tab' -- complete at once"
job {
	Assert-Far $Far.Editor.Line.Text -eq '    Microsoft'
}

macro "Keys'CtrlA Del l e t Space x = 1 Enter x . T o Tab' -- complete at once"
job {
	Assert-Far $Far.Editor.Line.Text -eq 'x.ToString'
}

# _160922_160602
# Ionide KO. VS OK.

macro "Keys'( ) . C Tab End Enter' -- complete string members C*"
job {
	Assert-Far $Far.Editor.Line.Text -eq 'x.ToString().CopyTo'
}

macro "Keys'Esc n' -- exit"
