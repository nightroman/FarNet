
namespace FarNet.Demo;

/// <summary>
/// Methods for calls by the command `fn:`.
/// </summary>
/// <example>
/// fn: module=FarNet.Demo; method=Message ;; name=John Doe; age=42
/// </example>
public static class Script
{
	public static void Message(string name = "unknown", int age = -1)
	{
		Far.Api.Message($"name: {name}, age: {age}");
	}
}
