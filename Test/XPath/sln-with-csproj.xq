
//File
[
	is-match(@Name, '(?i)\.sln$')
	and
	../File[is-match(@Name, '(?i)\.csproj$')]
]
