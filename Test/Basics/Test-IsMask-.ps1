
# checks
Assert-Far (!$Far.IsMaskValid(''))
Assert-Far (!$Far.IsMaskValid('|'))
Assert-Far ($Far.IsMaskValid('|z')) # inconsistent
Assert-Far ($Far.IsMaskValid('>'))
Assert-Far ($Far.IsMaskValid('?'))
Assert-Far ($Far.IsMaskValid('*'))
Assert-Far ($Far.IsMaskValid('//'))
Assert-Far (!$Far.IsMaskValid('/*/')) # mantis #2804

# none
Assert-Far (!$Far.IsMaskMatch('', ''))
Assert-Far (!$Far.IsMaskMatch('InputString', ''))

# none|exclude
Assert-Far ($Far.IsMaskMatch('InputString', '|z'))
Assert-Far (!$Far.IsMaskMatch('InputString', '|InputString'))

# no wildcards
Assert-Far ($Far.IsMaskMatch('InputString', 'INPUTSTRING'))
Assert-Far (!$Far.IsMaskMatch('InputString', 'nputStrin'))

# valid regex
Assert-Far ($Far.IsMaskMatch('InputString', '//i'))
Assert-Far ($Far.IsMaskMatch('InputString', '/INPUTSTRING/i'))
Assert-Far (!$Far.IsMaskMatch('InputString', '/INPUTSTRING/'))

# invalid regex
Assert-Far (!$Far.IsMaskMatch('InputString', '/*InputString/')) # mantis #2804

Assert-Far ($Far.IsMaskMatch('InputString', '*'))
Assert-Far ($Far.IsMaskMatch('InputString', '*Ts*'))
Assert-Far ($Far.IsMaskMatch('InputString', 'in*ng'))
Assert-Far ($Far.IsMaskMatch('InputString', 'i?*?g'))
Assert-Far ($Far.IsMaskMatch('InputString', 'input*'))
Assert-Far ($Far.IsMaskMatch('InputString', '*string'))
Assert-Far ($Far.IsMaskMatch('InputString', '*zz*;*xx*;*in*'))
Assert-Far ($Far.IsMaskMatch('InputString', '*zz*,*xx*,*in*'))
Assert-Far ($Far.IsMaskMatch('InputString', '*zz*;*xx*,*in*'))

Assert-Far ($Far.IsMaskMatch('C:\Bar\InputString', '*Ts*'))
Assert-Far (!$Far.IsMaskMatch('C:\InputString\Bar', '*Ts*'))

Assert-Far (!$Far.IsMaskMatch('InputString', '*|*'))
Assert-Far (!$Far.IsMaskMatch('InputString', '*|*Ts*'))
Assert-Far (!$Far.IsMaskMatch('InputString', '*|in*ng'))
Assert-Far (!$Far.IsMaskMatch('InputString', '*|i?*?g'))
Assert-Far (!$Far.IsMaskMatch('InputString', '*|input*'))
Assert-Far (!$Far.IsMaskMatch('InputString', '*|*string'))
Assert-Far (!$Far.IsMaskMatch('InputString', '*|*zz*;*xx*;*in*'))
Assert-Far (!$Far.IsMaskMatch('InputString', '*|*zz*,*xx*,*in*'))
Assert-Far (!$Far.IsMaskMatch('InputString', '*|*zz*;*xx*,*in*'))

Assert-Far ($Far.IsMaskMatch('bar.exe', '<exec>'))
