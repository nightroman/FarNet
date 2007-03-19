config = Release
farhome = c:\program files\far

copy:
	if not exist "$(farhome)\Lib" md "$(farhome)\Lib"
	if not exist "$(farhome)\Plugins\Far.NET" md "$(farhome)\Plugins\Far.NET"
	xcopy FarNetIntf\bin\$(config)\FarNetIntf.dll "$(farhome)\Lib" /y
	if $(config)==Release xcopy FarNetIntf\bin\$(config)\FarNetIntf.xml "$(farhome)\Lib" /y
	xcopy FarNetPlugin\$(config)\FarNetPlugin.dll "$(farhome)\Plugins\Far.NET" /y
	xcopy *.hlf "$(farhome)\Plugins\Far.NET" /y
