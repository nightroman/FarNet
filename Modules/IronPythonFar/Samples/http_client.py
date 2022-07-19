# This script uses %FARHOME%\FarNet\Modules\IronPythonFar\Lib libraries
# https://docs.python.org/3/library/http.client.html#module-http.client

import http.client

conn = http.client.HTTPSConnection("www.python.org")
conn.request("GET", "/")
r1 = conn.getresponse()
print(r1.status, r1.reason)

# get entire content
data1 = r1.read()
print(len(data1))
print(data1)

# get data in chunks
conn.request("GET", "/")
r1 = conn.getresponse()
while True:
    chunk = r1.read(100)
    if not chunk:
        break
    print(repr(chunk))

conn.close()
