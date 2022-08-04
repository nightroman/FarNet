# Configuration file sample

[_session.js.xml](_session.js.xml) is the session configuration file with some settings explained below.
If you remove/rename this file, sample scripts do not work for various reasons.

## DocumentAccessFlags

See [DocumentAccessFlags](https://microsoft.github.io/ClearScript/Reference/html/T_Microsoft_ClearScript_DocumentAccessFlags.htm)

```xml
  <DocumentAccessFlags>EnableAllLoading</DocumentAccessFlags>
```

This line changes access from the default `EnableFileLoading` to `EnableAllLoading`
which is the same as the combination `EnableFileLoading, EnableWebLoading`.

As a result, [WebLoadingAndSearchPath.js](WebLoadingAndSearchPath.js) can import the demo
module `const.js` from the GitHub repository.

## DocumentSearchPath

[DocumentSearchPath](https://microsoft.github.io/ClearScript/Reference/html/P_Microsoft_ClearScript_DocumentSettings_SearchPath.htm)

```xml
  <DocumentSearchPath>https://raw.githubusercontent.com/nightroman/FarNet/master/JavaScriptFar/Samples/modules/</DocumentSearchPath>
```

This line tells the engine where to search for referenced files,
a GitHub repository folder is this case.

As a result, [WebLoadingAndSearchPath.js](WebLoadingAndSearchPath.js) may specify the imported
module `const.js` just by name instead of the exact URL.

> Environment variables are expanded in this string.

## V8ScriptEngineFlags

See [V8ScriptEngineFlags](https://microsoft.github.io/ClearScript/Reference/html/T_Microsoft_ClearScript_V8_V8ScriptEngineFlags.htm)

```xml
  <V8ScriptEngineFlags>EnableTaskPromiseConversion, EnableStringifyEnhancements</V8ScriptEngineFlags>
```

`EnableTaskPromiseConversion` tells to enable conversion between .NET tasks and JavaScript promises.
As a result, in [TaskPromiseConversion.js](TaskPromiseConversion.js) functions
`sleep` and `job` formally return .NET tasks but `main` calls them as promises.

`EnableStringifyEnhancements` tells `JSON.stringify` to work with .NET objects as well as JavaScript.
As a result, [StringifyEnhancements.js](StringifyEnhancements.js) may stringify a .NET dictionary.
