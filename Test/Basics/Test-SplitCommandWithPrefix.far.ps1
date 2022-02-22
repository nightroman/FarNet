
# empty
$r = [PowerShellFar.Zoo]::SplitCommandWithPrefix('')
Assert-Far $r.Key -eq ''
Assert-Far $r.Value -eq ''

# spaces
$r = [PowerShellFar.Zoo]::SplitCommandWithPrefix(' ')
Assert-Far $r.Key -eq ' '
Assert-Far $r.Value -eq ''

# code
$r = [PowerShellFar.Zoo]::SplitCommandWithPrefix('line')
Assert-Far $r.Key -eq ''
Assert-Far $r.Value -eq 'line'

# code with spaces
$r = [PowerShellFar.Zoo]::SplitCommandWithPrefix(' line ')
Assert-Far $r.Key -eq ' '
Assert-Far $r.Value -eq 'line '

# prefix 1 and code
$r = [PowerShellFar.Zoo]::SplitCommandWithPrefix('ps:line')
Assert-Far $r.Key -eq 'ps:'
Assert-Far $r.Value -eq 'line'

# prefix 2 and code
$r = [PowerShellFar.Zoo]::SplitCommandWithPrefix('vps:line')
Assert-Far $r.Key -eq 'vps:'
Assert-Far $r.Value -eq 'line'

# prefix 1 with spaces and code
$r = [PowerShellFar.Zoo]::SplitCommandWithPrefix(' ps: line ')
Assert-Far $r.Key -eq ' ps: '
Assert-Far $r.Value -eq 'line '

# prefix 2 with spaces and code
$r = [PowerShellFar.Zoo]::SplitCommandWithPrefix(' vps: line ')
Assert-Far $r.Key -eq ' vps: '
Assert-Far $r.Value -eq 'line '

# unknown prefix
$r = [PowerShellFar.Zoo]::SplitCommandWithPrefix('foo:line')
Assert-Far $r.Key -eq ''
Assert-Far $r.Value -eq 'foo:line'

# unknown prefix with spaces
$r = [PowerShellFar.Zoo]::SplitCommandWithPrefix(' foo: line ')
Assert-Far $r.Key -eq ' '
Assert-Far $r.Value -eq 'foo: line '
