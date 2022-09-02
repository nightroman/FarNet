
$Data.File = 'c:/tmp/tmp.fsx'

Set-Content $Data.File @'
type T1 = { ``aa-1`` : string; ``aa-2`` : int }
let t1 = { ``aa-1`` = "it"; ``aa-2`` = 42 }
'@

job {
	Open-FarEditor $Data.File -DisableHistory
	$Far.Editor.GoToEnd($false)
}

### test 1

macro 'Keys [[t 1 . a a Tab]]'
macro 'Keys [[Down Enter]]'

job {
	$line = $Far.Editor[2]
	Assert-Far $line.Text -eq 't1.``aa-2``'
	$line.Text = ''
	$Far.Editor.Redraw() #!
}

### test 2, backticks _211111_fs

macro 'Keys [[t 1 . ` ` a a Tab]]'
macro 'Keys [[Down Enter]]'

job {
	$line = $Far.Editor[2]
	Assert-Far $line.Text -eq 't1.``aa-2``'
}

### test 3

job {
	$Far.Editor.SetText(@'
type T1 =
  static member P1 = ""
T1.P
'@)
	$Far.Editor.GoToEnd($false)
}
keys Tab
job {
	$line = $Far.Editor[2]
	Assert-Far $line.Text -eq 'T1.P1'
}

### test 4

job {
	$Far.Editor.SetText(@'
open FarNet
far.
'@)
	$Far.Editor.GoToEnd($false)
}
macro 'Keys [[Tab Enter]]'
job {
	$line = $Far.Editor[1]
	#! covers sorting of completions
	Assert-Far $line.Text -eq 'far.AnyEditor'
}

### end
job {
	$Far.Editor.Close()

	#! may fail due to "in use"
	Remove-Item -LiteralPath $Data.File -ErrorAction Ignore
}
