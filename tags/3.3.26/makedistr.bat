:: Makes distribution archive
:: It is .rar because Far.exe.config can be blocked in .zip, e.g. by gmail.
:: Zip command: pkzipc -add -dir %dst% *.*

@echo off
set version=3.3.26
set dst=Far.NET.%version%.rar

if exist tmp rmdir /s /q tmp
if exist %dst% del %dst%

xcopy "%FARHOME%\lib" tmp\Lib /i
xcopy "%FARHOME%\Plugins\Far.Net" tmp\Plugins\Far.NET /i
xcopy Plugins.NET tmp\Plugins.NET /i /s
copy Descript.ion tmp\Descript.ion
copy Far.exe.config tmp\Far.exe.config
copy History.txt tmp\History.txt
copy LICENSE tmp\LICENSE
copy Readme.txt tmp\Readme.txt

pushd tmp
rar a %dst% -m5 -r -s -t *.* > nul
popd

xcopy tmp\%dst% /y
rmdir /s /q tmp
