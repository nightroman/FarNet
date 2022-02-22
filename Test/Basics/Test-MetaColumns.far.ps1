<#
.Synopsis
	Table column mapping.

.Description
	In V3 ctp2 errors are not created when the test is invoked in a batch test
	and created when it is invoked alone. V2 creates errors always. Weird, so
	we require no errors before the test and remove errors if(!) they are
	created.
#>

if ($Error) {throw 'Please remove errors.'}

$default = "N", "Z", "O", "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9"

# all default kinds
$columns = $default
$metas = [PowerShellFar.Zoo]::TablePanelSetupColumns($columns)
for($1 = 0; $1 -lt $default.Length; ++$1) {
	$meta = $metas[$1]
	$data = $default[$1]
	Assert-Far @(
		$meta.Property -eq $data
		$meta.Kind -eq $data
	)
}

# custom
$columns = @{ k = 'C0'; e = 'name'; l = 'title'; w = -9 }
$metas = [PowerShellFar.Zoo]::TablePanelSetupColumns($columns)

### Test input errors

$4 = ''
$columns = $default + 'ExtraColumn'
try { [PowerShellFar.Zoo]::TablePanelSetupColumns($columns) } catch { $4 = "$_" }
Assert-Far ($4 -match "Too many columns.") -Message "Actual [[$4]]."
if ($Error) {$Error.RemoveAt(0)}

$4 = ''
$columns = @{ Kind = 'C'; Expression = 'Foo' }
try { [PowerShellFar.Zoo]::TablePanelSetupColumns($columns) } catch { $4 = "$_" }
Assert-Far ($4 -match "Invalid column kind: C")
if ($Error) {$Error.RemoveAt(0)}

$4 = ''
$columns = @{ Kind = 'C1'; Expression = 'Foo' }
try { [PowerShellFar.Zoo]::TablePanelSetupColumns($columns) } catch { $4 = "$_" }
Assert-Far ($4 -match "Invalid column kind: C1. Expected: C0")
if ($Error) {$Error.RemoveAt(0)}

$4 = ''
$columns = @{ Kind = 'C0'; Expression = 'Foo' }, @{ Kind = 'C2'; Expression = 'Foo' }
try { [PowerShellFar.Zoo]::TablePanelSetupColumns($columns) } catch { $4 = "$_" }
Assert-Far ($4 -match "Invalid column kind: C2. Expected: C1")
if ($Error) {$Error.RemoveAt(0)}
