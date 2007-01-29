import re
import sys
stdafx=re.compile(r'\s+stdafx.cpp')
ordinal=re.compile(r"^\s+\'(.*)\'.*")
qualified=re.compile(r'\w+::')
for line in file("input.txt"):
    if not stdafx.match(line):
        core=ordinal.match(line).group(1)
        core=qualified.sub('', core).replace('__gc','')
        if len(sys.argv)>=2 and sys.argv[1]=='d':
            print core+';'
        else:
            print core+'{\r\n}'