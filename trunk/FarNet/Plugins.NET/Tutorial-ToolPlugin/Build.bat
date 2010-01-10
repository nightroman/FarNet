
rem Build plugin (Hello.dll)
call ..\Build-CS /out:Hello.dll Hello.cs

rem Build resource files (Hello.resources, Hello.ru.resources)
"%PROGRAMFILES%\Microsoft SDKs\Windows\v6.0A\bin\ResGen.exe" Hello.restext
"%PROGRAMFILES%\Microsoft SDKs\Windows\v6.0A\bin\ResGen.exe" Hello.ru.restext
