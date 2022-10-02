using FarNet;

namespace Script;

public static class Demo
{
	// Static method with string parameters.
	public static void Message(string name, string age)
	{
		Far.Api.Message($"name: {name}, age: {age}");
	}
}
