
//File
[
	is-match(@Name, '(?i)\.sln$')
	and
	not(../File[is-match(@Name, '(?i)\.csproj$')])
]
