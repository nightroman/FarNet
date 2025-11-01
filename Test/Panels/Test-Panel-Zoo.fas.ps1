<#
.Synopsis
	Test Zoo data and a list panel.
#>


function Get-Block-Add
{
	macro 'Keys"F7 AltV 1 2 3 AltN I n t 1 AltT i n t Enter"'
	job {
		Assert-Far -FileName 'Int1'
		Assert-Far (Get-FarItem).Value -eq 123
	}
}

function Get-Block-Delete
{
	job { $global:FileToRemove = $__.CurrentFile.Name }
	keys Del
	job {
		Assert-Far -Dialog
		Assert-Far $__[0].Text -eq 'Delete'
	}
	keys Enter
	job {
		Assert-Far ($__.CurrentFile.Name -ne $global:FileToRemove)
		Remove-Variable -Name FileToRemove -Scope global
	}
}

# open data panel
job {
	& "$env:FarNetCode\Samples\Tests\Test-Zoo-.ps1"
	Remove-Item "$HOME\Test-Zoo.clixml"
}
keys Down
job { Assert-Far -FileName '.NET data' }

# step into, ensure '..', set global '$this'
keys Enter
job {
	Find-FarFile '..'
	$global:e = $__.Value
}

# fixed: try to enter new value on dots;
macro 'Keys"= 1 Enter"'

### bool_
job {
	Find-FarFile 'bool_'
	Assert-Far $e.bool_ -eq $true
}
macro 'Keys"Enter 0 Enter"'
job {
	Assert-Far $e.bool_ -eq $false
}
macro 'Keys"Enter 1 Enter"'
job {
	Assert-Far $e.bool_ -eq $true
}

### byte_
keys Down
job {
	Assert-Far -FileName 'byte_'
	Assert-Far $e.byte_ -eq ([byte]11)
}
macro 'Keys"Enter 9 9 Enter"'
job {
	Assert-Far $e.byte_ -eq ([byte]99)
}

### bytes
keys Down
job {
	Assert-Far -FileName 'bytes'
	Assert-Far $e.bytes.Count -eq 2
}
macro 'Keys"F4 CtrlEnd 3 3 F2 Esc"'
job {
	Assert-Far $e.bytes.Count -eq 3
	Assert-Far $e.bytes[2] -eq ([byte]33)
	Assert-Far ($e.bytes -is [byte[]])
}

### char_
keys Down
job {
	Assert-Far -FileName 'char_'
	Assert-Far $e.char_ -eq ([char]'c')
}
macro 'Keys"Enter z Enter"'
job {
	Assert-Far $e.char_ -eq ([char]'z')
}

### DateTime_
keys Down
job {
	Assert-Far -FileName 'DateTime_'
	Assert-Far $e.DateTime_ -eq ([datetime]'2000-11-22')
}
macro 'Keys"Enter Right BS 9 Enter"'
job { Assert-Far $e.DateTime_ -eq ([datetime]'2000-11-29') }
macro 'Keys"Enter Right Space 1 1 : 1 1 Enter"'
job {
	Assert-Far $e.DateTime_ -eq ([datetime]'2000-11-29 11:11')
}

### decimal_
keys Down
job {
	Assert-Far -FileName 'decimal_'
	Assert-Far $e.decimal_ -eq 0.12345678901234567890d
}
macro 'Keys"Enter Right 1 2 Enter"'
job {
	Assert-Far $e.decimal_ -eq 0.1234567890123456789012d
}

### double_
keys Down
job {
	Assert-Far -FileName 'double_'
	Assert-Far $e.double_ -eq 3.1415
}
macro 'Keys"Enter Right BS Enter"'
job {
	Assert-Far $e.double_ -eq 3.141
}

### float_
keys Down
job {
	Assert-Far -FileName 'float_'
	Assert-Far ([math]::Abs($e.float_ - 3.1415) -le 1e-6)
}
macro 'Keys"Enter Right BS Enter"'
job {
	Assert-Far @(
		[math]::Abs($e.float_ - 3.141) -le 1e-6
		$e.float_ -is [float]
	)
}

### Guid_
keys Down
job {
	Assert-Far -FileName 'Guid_'
	Assert-Far $e.Guid_ -eq ([guid]'8e3867a3-8586-11d1-b16a-00c0f0283628')
}
macro 'Keys"Enter Right BS 0 Enter"'
job {
	Assert-Far $e.Guid_ -eq ([guid]'8e3867a3-8586-11d1-b16a-00c0f0283620')
}

### int_
keys Down
job {
	Assert-Far -FileName 'int_'
	Assert-Far $e.int_ -eq 11
}
macro 'Keys"Enter 9 9 Enter"'
job {
	Assert-Far $e.int_ -eq 99
}

### long_
keys Down
job {
	Assert-Far -FileName 'long_'
	Assert-Far $e.long_ -eq 11L
}
macro 'Keys"Enter 9 9 Enter"'
job {
	Assert-Far $e.long_ -eq 99L
}

### sbyte_
keys Down
job {
	Assert-Far -FileName 'sbyte_'
	Assert-Far $e.sbyte_ -eq ([sbyte]-11)
}
macro 'Keys"Enter - 9 9 Enter"'
job {
	Assert-Far $e.sbyte_ -eq ([sbyte]-99)
}

### short_
keys Down
job {
	Assert-Far -FileName 'short_'
	Assert-Far $e.short_ -eq ([int16]11)
}
macro 'Keys"Enter 9 9 Enter"'
job {
	Assert-Far $e.short_ -eq ([int16]99)
}

### string_
keys Down
job { Assert-Far -FileName 'string_' }

# edit in thr command line
macro 'Keys"Enter z Enter"'
job { Assert-Far $e.string_ -eq 'z' }

# edit in the editor
macro 'Keys"F4 CtrlEnd Enter x F2 Esc"'
job {
	Assert-Far $e.string_ -eq "z`r`nx"
}

# fixed: unwanted dialog about missed file on edit + escape
keys F4
job { Assert-Far -Editor }
keys Esc
job { Assert-Far -Panels }

### strings
keys Down
job {
	Assert-Far -FileName 'strings'
	Assert-Far $e.strings.Count -eq 2
}
macro 'Keys"F4 CtrlEnd R o c k s Enter F2 Esc"'
job {
	Assert-Far $e.strings.Count -eq 3
	Assert-Far "$($e.strings)" -eq 'Power Shell Rocks'
	Assert-Far ($e.strings -is [string[]])
}

### TimeSpan_
keys Down
job {
	Assert-Far -FileName 'TimeSpan_'
	Assert-Far $e.TimeSpan_ -eq ([timespan]'01:01:01')
}
macro 'Keys"Enter Right BS 9 Enter"'
job {
	Assert-Far $e.TimeSpan_ -eq ([timespan]'01:01:09')
}

### uint_
keys Down
job {
	Assert-Far -FileName 'uint_'
	Assert-Far $e.uint_ -eq ([uint32]11)
}
macro 'Keys"Enter 9 9 Enter"'
job {
	Assert-Far $e.uint_ -eq ([uint32]99)
}

### ulong_
keys Down
job {
	Assert-Far -FileName 'ulong_'
	Assert-Far $e.ulong_ -eq ([uint64]11)
}
macro 'Keys"Enter 9 9 Enter"'
job {
	Assert-Far $e.ulong_ -eq ([uint64]99)
}

### ushort_
keys Down
job {
	Assert-Far -FileName 'ushort_'
	Assert-Far $e.ushort_ -eq ([uint16]11)
}
macro 'Keys"Enter 9 9 Enter"'
job {
	Assert-Far $e.ushort_ -eq ([uint16]99)
}

### enumFlags
keys Down
job {
	Assert-Far -FileName 'enumFlags'
	Assert-Far ($e.enumFlags -eq 'Flag1,Flag2,Flag3')
}
macro 'Keys"Enter F l a g 1 Enter"'
job {
	Assert-Far @(
		$e.enumFlags -eq 'Flag1'
		$e.enumFlags.GetType().IsEnum
	)
}

### enumValue
keys Down
job {
	Assert-Far -FileName 'enumValue'
	Assert-Far ($e.enumValue -eq 'Value1')
}
macro 'Keys"Enter Right BS 2 Enter"'
job {
	Assert-Far @(
		$e.enumValue -eq 'Value2'
		$e.enumFlags.GetType().IsEnum
	)
}

# step out and go deserialized
macro 'Keys"Esc Down"'

### Deserialized .NET data
job { Assert-Far -FileName '.NET data (Imported)' }
keys Enter
# set null value
job {
	Find-FarFile 'name'
	Assert-Far ($__.Value.Name -ne $null)
}
macro 'Keys"ShiftDel Enter"'#??????
job {
	Assert-Far -FileName name
	Assert-Far $__.Value.Name -eq $null
}

# try to delete and check it is not deleted
macro 'Keys"Del Enter"'#??????
job {
	Assert-Far -FileName name
}

# add and delete a property
Get-Block-Add
Get-Block-Delete

### Deserialized bytes
job {
	Find-FarFile 'bytes'
	Assert-Far -FileDescription '{11, 22}'
}
# edit
keys F4
job {
	Assert-Far -Editor
	Assert-Far $__.Count -eq 3
	$__.Close()
}
job {
	Assert-Far -Panels
}

### Deserialized strings
job {
	Find-FarFile 'strings'
	Assert-Far -FileDescription '{Power, Shell}'
}
# edit
keys F4
job {
	Assert-Far -Editor
	Assert-Far $__.Count -eq 3
	$__.Close()
}
job {
	Assert-Far -Panels
}

# step out
keys Esc
# with null
job { Find-FarFile 'With null data' }
keys Enter
# set data 1
job {
	Find-FarFile 'Data1'
	Assert-Far -FileDescription '<null>'
}
macro 'Keys"Enter f o o Enter"'
job { Assert-Far -FileDescription 'foo' }
# exit
keys Esc
# exit
keys Esc
job {
	Remove-Variable -Scope global e
}
