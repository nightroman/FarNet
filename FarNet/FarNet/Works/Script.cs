﻿// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet.Tools;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FarNet.Works;
#pragma warning disable 1591

public static class Script
{
	const string
		KeyMethod = "method",
		KeyModule = "module",
		KeyScript = "script",
		KeyUnload = "unload";

	class ScriptParameters
	{
		public string ModuleName = null!;
		public string ScriptName = null!;
		public string TypeName = null!;
		public string MethodName = null!;
		public bool Unload;
	}

	public static void InvokeCommand()
	{
		Task.Run(async () =>
		{
			var ui = new InputBox("Command", "FarNet command");
			ui.Edit.History = "FarNet command";
			ui.Edit.UseLastHistory = true;

			var text = await ui.ShowAsync();

			if (string.IsNullOrEmpty(text))
				return;

			await Tasks.Job(() =>
			{
				try
				{
					Far.Api.InvokeCommand(text);
				}
				catch (Exception ex)
				{
					Far.Api.ShowError(null, ex);
				}
			});
		});
	}

	static ScriptParameters ParseScriptParameters(string text)
	{
		var sb = Kit.ParseParameters(text);
		var res = new ScriptParameters();

		// script
		if (sb.TryGetValue(KeyScript, out object? script))
		{
			res.ScriptName = script.ToString()!;
			sb.Remove(KeyScript);
		}

		// module
		if (sb.TryGetValue(KeyModule, out object? module))
		{
			res.ModuleName = module.ToString()!;
			sb.Remove(KeyModule);
		}

		if (script is null && module is null)
			throw new InvalidOperationException("Missing required parameter 'script' or 'module'.");

		if (script != null && module != null)
			throw new InvalidOperationException("Parameters 'script' and 'module' cannot be used together.");

		// method
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

		// unload
		if (sb.TryGetValue(KeyUnload, out object? unload))
		{
			res.Unload = bool.Parse(unload.ToString()!);
			sb.Remove(KeyUnload);
		}

		if (sb.Count > 0)
			throw new InvalidOperationException($"Unknown script parameters: {sb}");

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

		var sb = text is null ? null : Kit.ParseParameters(text);

		var res = new object?[methodParameters.Length];
		for (int i = 0; i < res.Length; i++)
		{
			var parameter = methodParameters[i];

			if (sb != null && sb.TryGetValue(parameter.Name!, out object? value))
			{
				sb.Remove(parameter.Name!);

				if (parameter.ParameterType == typeof(string))
				{
					res[i] = value;
				}
				else
				{
					var converter = TypeDescriptor.GetConverter(parameter.ParameterType);
					res[i] = converter.ConvertFromInvariantString(value.ToString()!);
				}
			}
			else if (parameter.HasDefaultValue)
			{
				res[i] = parameter.DefaultValue;
			}
			else if (parameter.ParameterType.IsValueType)
			{
				res[i] = Activator.CreateInstance(parameter.ParameterType);
			}
		}

		if (sb?.Count > 0)
			throw new InvalidOperationException($"Unknown method parameter '{sb.Keys.OfType<object>().First()}'.");

		return res;
	}

	public static void InvokeScript(string command)
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

		// get script parameters
		var scriptParameters = ParseScriptParameters(scriptParametersText);

		// load assembly
		AssemblyLoadContext2? loader = null;
		Assembly assembly;
		if (scriptParameters.ModuleName is null)
		{
			var dll = $"{Environment.GetEnvironmentVariable("FARHOME")}\\FarNet\\Scripts\\{scriptParameters.ScriptName}\\{scriptParameters.ScriptName}.dll";
			loader = new AssemblyLoadContext2(dll, scriptParameters.Unload);
			assembly = loader.LoadFromAssemblyPath(dll);
		}
		else
		{
			var manager = Far.Api.GetModuleManager(scriptParameters.ModuleName);
			assembly = manager.LoadAssembly(true);
		}

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
			try
			{
				method.Invoke(instance, methodParameters);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException ?? ex;
			}
		}
		finally
		{
			if (scriptParameters.Unload && loader != null)
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
