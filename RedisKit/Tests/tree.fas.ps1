
job {
	Import-Module $PSScriptRoot\zoo.psm1
	make_test_tree
}

### special case `root=:` for `:*`

job {
	$Far.InvokeCommand('rk:tree root=:')
}
job {
	Find-FarFile test-tree-file-in-empty-folder-name
	$Far.Panel.Close()
}

### test-tree-file-in-empty-folder-name

job {
	$Far.InvokeCommand('rk:tree')
}

job {
	Find-FarFile ': (1)'
}
keys Enter
job {
	$files = $Far.Panel.GetFiles()
	Assert-Far $files.Count -eq 1
	Assert-Far $files[0].Name -eq test-tree-file-in-empty-folder-name
}
keys Enter # dots
job {
	Assert-Far -FileName ': (1)'
}
keys Del
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Delete 1 folder(s), 0 key(s):'
	Assert-Far $Far.Dialog[2].Text -eq ': (1)'
}
keys Enter

### test-tree: (9)

job {
	Find-FarFile 'test-tree: (9)'
}
keys Enter
job {
	$files = $Far.Panel.GetFiles()
	Assert-Far $files.Count -eq 4
	Assert-Far $files[0].Name -eq ': (3)'
	Assert-Far $files[1].Name -eq 'folder1: (4)'
	Assert-Far $files[2].Name -eq 'file-in-root-1'
	Assert-Far $files[3].Name -eq 'file-in-root-2'

	Find-FarFile ': (3)'
}
keys Enter
job {
	$files = $Far.Panel.GetFiles()
	Assert-Far $files.Count -eq 3
	Assert-Far $files[0].Name -eq ''
	Assert-Far $files[1].Name -eq 'file-in-empty-1'
	Assert-Far $files[2].Name -eq 'file-in-empty-2'
}
keys CtrlPgUp
job {
	Assert-Far -FileName ': (3)'

	Find-FarFile 'folder1: (4)'
}
keys Enter
job {
	$files = $Far.Panel.GetFiles()
	Assert-Far $files.Count -eq 3
	Assert-Far $files[0].Name -eq 'folder2: (2)'
	Assert-Far $files[1].Name -eq 'file-in-folder1-1'
	Assert-Far $files[2].Name -eq 'file-in-folder1-2'
}
keys Down Ins Ins Ins Del
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Delete 1 folder(s), 2 key(s):'
	Assert-Far $Far.Dialog[2].Text -eq 'folder2: (2)'
	Assert-Far $Far.Dialog[3].Text -eq 'file-in-folder1-1'
	Assert-Far $Far.Dialog[4].Text -eq 'file-in-folder1-2'
}
keys Enter
job {
	Assert-Far $Far.Panel.GetFiles().Count -eq 0
}
keys CtrlPgUp
job {
	$files = $Far.Panel.GetFiles()
	Assert-Far $files.Count -eq 3
	Assert-Far $files[0].Name -eq ': (3)'
	Assert-Far $files[1].Name -eq 'file-in-root-1'
	Assert-Far $files[2].Name -eq 'file-in-root-2'
}
keys CtrlPgUp
job {
	#! file name changed but PostData helps finding it
	Assert-Far -FileName 'test-tree: (5)'
}
keys Del
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Delete 1 folder(s), 0 key(s):'
	Assert-Far $Far.Dialog[2].Text -eq 'test-tree: (5)'
}
keys Enter Esc
