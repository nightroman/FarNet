// This script loads and uses MyLibForJS.dll created "for JS" because some code
// is much easier in C#. MyLibForJS.dll represents one of your "real projects".
// Here it is a mock compiled by `test-MyLibForJS.ps1`.

// load my library
const dll = clr.System.Environment.ExpandEnvironmentVariables('%TEMP%\\MyLibForJS.dll')
clr.System.Reflection.Assembly.LoadFrom(dll)

// import my library types
const lib = host.lib('MyLibForJS')

// call my type static method
let res = lib.MyLibForJS.DifficultForJS.Job1()

// create my type object and call its method
const obj = new lib.MyLibForJS.DifficultForJS()
res += ', ' + obj.Job2()

// show result
far.Message(res)
