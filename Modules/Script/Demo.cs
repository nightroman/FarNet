using FarNet;
using System;

namespace Script;

// Type with static fn-methods.
public static class Demo
{
	// For testing static construction.
	static Demo()
	{
		var count = int.Parse(Environment.GetEnvironmentVariable("SCRIPT_DEMO_COUNT") ?? "0");
		Environment.SetEnvironmentVariable("SCRIPT_DEMO_COUNT", (count + 1).ToString());
	}

	// fn: script=Script; method=Script.Demo.Message :: name=John Doe; age=42
	public static void Message(string name = "unknown", int age = -1)
	{
		if (string.IsNullOrEmpty(name))
			return;

		if (age < 0)
			throw new ArgumentException("Age cannot be negative.");

		Far.Api.Message($"name: {name}, age: {age}");
	}
}
