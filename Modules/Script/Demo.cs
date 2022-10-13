using FarNet;

namespace Script;

// Type with static fn-methods.
public static class Demo
{
	// fn: script=Script; method=Script.Demo.Message :: name=John Doe; age=42
	public static void Message(string name = "unknown", int age = -1)
	{
		Far.Api.Message($"name: {name}, age: {age}");
	}
}
