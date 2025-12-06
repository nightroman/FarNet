<#
.Synopsis
	Test IFar.GetSetting()

.Description
	See C:\-\GIT\FarManager3\unicode_far\config.cpp
#>

### Confirmations
Assert-Far (1 -eq $Far.GetSetting('Confirmations', 'Copy'))
Assert-Far (1 -eq $Far.GetSetting('Confirmations', 'Move'))
Assert-Far (1 -eq $Far.GetSetting('Confirmations', 'RO'))
Assert-Far (1 -eq $Far.GetSetting('Confirmations', 'Drag'))
Assert-Far (1 -eq $Far.GetSetting('Confirmations', 'Delete'))
Assert-Far (1 -eq $Far.GetSetting('Confirmations', 'DeleteFolder'))
Assert-Far (1 -eq $Far.GetSetting('Confirmations', 'Esc'))
Assert-Far (1 -eq $Far.GetSetting('Confirmations', 'HistoryClear'))
Assert-Far (0 -eq $Far.GetSetting('Confirmations', 'Exit'))
Assert-Far (1 -eq $Far.GetSetting('Confirmations', 'RemoveConnection'))

### System
Assert-Far (1 -eq $Far.GetSetting('System', 'DeleteToRecycleBin'))
Assert-Far (1 -eq $Far.GetSetting('System', 'CopyOpened'))
Assert-Far (0x20000 -eq $Far.GetSetting('System', 'PluginMaxReadData'))
Assert-Far (1 -eq $Far.GetSetting('System', 'ScanJunction'))

### Panels
Assert-Far (0 -eq $Far.GetSetting('Panels', 'ShowHidden'))

### Editor
# 3.0.4367 WordDiv is same for Editor and System
# _180213 my own: $
$WordDiv = @'
~!%^&*()+|{}:"<>?`=\[];',./
'@
Assert-Far ($WordDiv -eq $Far.GetSetting('Editor', 'WordDiv'))
Assert-Far ($WordDiv -eq $Far.GetSetting('System', 'WordDiv'))

### Screen
Assert-Far ($Far.GetSetting('Screen', 'KeyBar') -is [long])

### Dialog
Assert-Far ($Far.GetSetting('Dialog', 'EditBlock') -eq 0)
Assert-Far ($Far.GetSetting('Dialog', 'EULBsClear') -eq 1)
Assert-Far ($Far.GetSetting('Dialog', 'DelRemovesBlocks') -eq 1)

### Interface
Assert-Far ($Far.GetSetting('Interface', 'ShowMenuBar') -eq 0)

### PanelLayout
Assert-Far ($Far.GetSetting('PanelLayout', 'ColumnTitles') -eq 1)
Assert-Far ($Far.GetSetting('PanelLayout', 'StatusLine') -eq 1)
Assert-Far ($Far.GetSetting('PanelLayout', 'SortMode') -is [long])

### Missing
$4 = ''
try { $Far.GetSetting('Confirmations', 'zzz') }
catch { $4 = "$_" }
Assert-Far ($4 -like "*Cannot get setting: set = 'Confirmations' name = 'zzz'*")
$Error.RemoveAt(0)
