@echo off
set version=3.2.6
set dst=Far.Net.%version%.rar

if exist tmp rmdir /s /q tmp
if exist %dst% del %dst%

xcopy "%FARHOME%\lib" tmp\lib /i
xcopy "%FARHOME%\Plugins\Far.Net" tmp\Plugins\Far.Net /i
xcopy ..\Plugins.Net tmp\Plugins.Net /i /s
xcopy resources tmp /i /s

pushd tmp
rar a %dst% -m5 -r -s -t *.* > nul
popd

xcopy tmp\%dst% /y
rmdir /s /q tmp
