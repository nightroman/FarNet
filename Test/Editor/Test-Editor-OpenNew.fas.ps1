<#
.Synopsis
	Tests how new files are opened in editors.

.Description
	http://bugs.farmanager.com/view.php?id=1988

	Хотелось бы, чтобы редактор открыл новый пустой файл, если имя файла не
	задано (NULL или ""). В противном случае приходится придумывать имя нового
	файла, а это не такая тривиальная задача, если подумать - имя должно быть
	1) правильное 2) уникальное 3) желательно вразумительное.
#>

### Far 3.0.2400 allows opening with null name

job {
	# WAS FileName = $null, IsNew = $false -> Cannot open the file '<null>'
	# WAS FileName = $null, IsNew = $true -> Cannot open the file '<null>'
	# NOW FileName = $null -> OK
	$e = $Far.CreateEditor()
	$e.DisableHistory = $true
	$e.Open()
}
job {
	Assert-Far -EditorFileName '*\New_File_*.txt'
	$Far.Editor.Close()
}
job {
	Assert-Far -Panels
}

### Far 3.0.2400 allows opening with empty name

job {
	# WAS FileName = '', IsNew = $false -> Cannot open the file ''
	# WAS FileName = '', IsNew = $true -> Cannot open the file ''
	# NOW FileName = '' -> OK
	$e = $Far.CreateEditor()
	$e.DisableHistory = $true
	$e.FileName = ''
	$e.Open()
}
job {
	Assert-Far -EditorFileName '*\New_File_*.txt'
	$Far.Editor.Close()
}
job {
	Assert-Far -Panels
}

### Cmdlet Open-FarEditor
# The same as above but may have subtle differences.

job {
	Open-FarEditor
}
job {
	Assert-Far -EditorFileName '*\New_File_*.txt'
	$Far.Editor.Close()
}
job {
	Assert-Far -Panels
}

### Open missing file as new

job {
	$e = $Far.CreateEditor()
	$e.DisableHistory = $true
	$e.FileName = 'feb9343c-6242-402f-aa0b-5427501b2942'
	$e.Open()
}
job {
	Assert-Far -EditorFileName *\feb9343c-6242-402f-aa0b-5427501b2942
	$Far.Editor.Close()
}
job {
	Assert-Far -Panels
}

<#
Другой недостаток. Неважно по каким причинам, но юзер указал имя папки, а не
файла для открытия в редакторе. Фар показывает диалог об ошибке. А это должно
быть дело плагина решать, показывать ли такой диалог и с каким сообщением, если
показывать.
#>

run {
	### FileName = Folder
	$ko = $null
	try {
		$e = $Far.CreateEditor()
		$e.FileName = 'c:\temp\'
		$e.Open()
	}
	catch {
		$ko = $_
	}
	Assert-Far ($ko -ne $null) -Message "Expected to fail"
	Assert-Far ($ko.ToString() -like "*Cannot open the file 'c:\temp\'*") -Message $ko
	$global:Error.Clear()
}

job {
	### Checked flaw - Far shows msg
	$dialog = $Far.Dialog
	Assert-Far ($dialog -ne $null)
	Assert-Far $dialog[1].Text -eq "It is impossible to edit the folder"
}

keys Esc
