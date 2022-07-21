using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;

namespace JavaScriptFar;

static class ScriptEngines
{
	internal static ScriptEngine V8ScriptEngine(bool isDebug)
	{
		var flags = isDebug ? V8ScriptEngineFlags.EnableDebugging | V8ScriptEngineFlags.AwaitDebuggerAndPauseOnStart : V8ScriptEngineFlags.None;
		return new V8ScriptEngine(Res.MyName, flags)
		{
			AllowReflection = true,
			SuppressExtensionMethodEnumeration = true
		};
	}
}
