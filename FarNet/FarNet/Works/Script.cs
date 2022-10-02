// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FarNet.Works;
#pragma warning disable 1591

public static class Script
{
	const string
		KeyMethod = "method",
		KeyScript = "script",
		KeyUnload = "unload";

	class ScriptParameters
	{
		public string ScriptName = null!;
		public string TypeName = null!;
		public string MethodName = null!;
		public bool Unload;
	}

	public static void InvokeCommand()
	{
		var command = Far.Api.Input("Command", "FarNet command", "FarNet command");
		if (command != null)
			Far.Api.InvokeCommand(command);
	}

	static ScriptParameters ParseScriptParameters(string text)
	{
		var sb = new DbConnectionStringBuilder { ConnectionString = text };
		var res = new ScriptParameters();

		if (sb.TryGetValue(KeyScript, out object? script))
		{
			res.ScriptName = script.ToString()!;
			sb.Remove(KeyScript);
		}
		else
		{
			throw new InvalidOperationException("Missing required parameter 'script'.");
		}

		if (sb.TryGetValue(KeyMethod, out object? method))
		{
			var name = method.ToString()!;
			int index = name.LastIndexOf('.');
			if (index < 0)
				throw new InvalidOperationException("Invalid method name.");

			res.TypeName = name[..index];
			res.MethodName = name[(index + 1)..];
			sb.Remove(KeyMethod);
		}
		else
		{
			throw new InvalidOperationException("Missing required parameter 'method'.");
		}

		if (sb.TryGetValue(KeyUnload, out object? unload))
		{
			res.Unload = bool.Parse(unload.ToString()!);
			sb.Remove(KeyUnload);
		}

		if (sb.Count > 0)
			throw new InvalidOperationException($"Unknown script parameter '{sb.Keys.OfType<object>().First()}'.");

		return res;
	}

	static object?[]? ParseMethodParameters(MethodInfo method, string? text)
	{
		var methodParameters = method.GetParameters();
		if (methodParameters.Length == 0)
		{
			if (text != null)
				throw new InvalidOperationException("Method does not have parameters.");

			return null;
		}

		var sb = text is null ? null : new DbConnectionStringBuilder { ConnectionString = text };

		var res = new object?[methodParameters.Length];
		for (int i = 0; i < res.Length; i++)
		{
			var parameter = methodParameters[i];
			if (parameter.ParameterType != typeof(string))
				throw new InvalidOperationException($"Method parameters should be strings.");

			if (sb != null && sb.TryGetValue(parameter.Name!, out object? value))
			{
				res[i] = value;
				sb.Remove(parameter.Name!);
			}
		}

		if (sb?.Count > 0)
			throw new InvalidOperationException($"Unknown method parameter '{sb.Keys.OfType<object>().First()}'.");

		return res;
	}

	public static void Invoke(string command)
	{
		// get script and method parts of the command
		string scriptParametersText;
		string? methodParametersText;
		var index = command.IndexOf("::");
		if (index < 0)
		{
			scriptParametersText = command;
			methodParametersText = null;
		}
		else
		{
			scriptParametersText = command[..index];
			methodParametersText = command[(index + 2)..];
		}

		// get script parameters and load assembly
		var scriptParameters = ParseScriptParameters(scriptParametersText);
		var dll = $"{Environment.GetEnvironmentVariable("FARHOME")}\\FarNet\\Scripts\\{scriptParameters.ScriptName}\\{scriptParameters.ScriptName}.dll";
		var loader = new AssemblyLoadContext2(dll, scriptParameters.Unload);
		var assembly = loader.LoadFromAssemblyPath(dll);

		// to be finally unloaded
		try
		{
			// find type
			var type = assembly.GetType(scriptParameters.TypeName);
			if (type is null)
				throw new Exception($"Cannot find type '{scriptParameters.TypeName}'.");

			// find method
			var method = type.GetMethod(scriptParameters.MethodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
			if (method is null)
				throw new Exception($"Cannot find method '{scriptParameters.MethodName}'.");

			// parse method parameters
			var methodParameters = ParseMethodParameters(method, methodParametersText);

			// done with parsing, create an instance
			object? instance;
			if (method.IsStatic)
			{
				instance = null;
			}
			else
			{
				try
				{
					instance = Activator.CreateInstance(type);
				}
				catch (TargetInvocationException ex)
				{
					throw ex.InnerException!;
				}
			}

			// go!
			method.Invoke(instance, methodParameters);
		}
		finally
		{
			if (scriptParameters.Unload)
			{
				loader.Unload();
				Task.Run(() =>
				{
					var weakRef = new WeakReference(loader, true);
					for (int i = 0; weakRef.IsAlive && i < 10; i++)
					{
						GC.Collect();
						GC.WaitForPendingFinalizers();
					}
				});
			}
		}
	}
}
