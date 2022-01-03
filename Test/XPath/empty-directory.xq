
//Directory
[
	not(Directory | File)
	and
	not((../.. | ../../..)/*[@Name = '.svn'])
]
