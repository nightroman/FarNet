using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;

namespace JavaScriptFar;

static class ScriptEngines
{
	internal static ScriptEngine V8ScriptEngine()
	{
		return new V8ScriptEngine("JavaScriptFar")
		{
			AllowReflection = true,
			SuppressExtensionMethodEnumeration = true
		};
	}
}
