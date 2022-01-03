
//File[
	upper-case(substring(@Name, string-length(@Name) - 6)) = '.CSPROJ'
	and
	../*[upper-case(substring(@Name, string-length(@Name) - 3)) = '.SLN']
]
