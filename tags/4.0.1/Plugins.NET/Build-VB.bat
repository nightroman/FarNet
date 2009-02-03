
:: Synopsis : Build FarNet plugin library from VB source
:: Example  : Build-VB MyPlugin.vb
:: Output   : MyPlugin.dll

set dotnet=%WINDIR%\Microsoft.NET\Framework\v2.0.50727
set farlib="%FARHOME%\lib\FarNet.dll"
%dotnet%\vbc /reference:%farlib% /target:library %*
