
:: Synopsis : Build Far.Net plugin library from JS source
:: Example  : Build-JS MyPlugin.js
:: Output   : MyPlugin.dll

set jsc=%WINDIR%\Microsoft.NET\Framework\v2.0.50727\jsc.exe
set farlib="%FARHOME%\lib\FarNetIntf.dll"
%jsc% /reference:%farlib% /target:library %*
