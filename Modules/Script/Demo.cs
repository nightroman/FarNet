using FarNet;

namespace Script;

public static class Demo
{
	// Static method with parameters.
	public static void Message(string name = "unknown", int age = -1)
	{
		Far.Api.Message($"name: {name}, age: {age}");
	}
}
