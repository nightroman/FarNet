// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet.Tools;
using System;
using System.Collections.Generic;
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

	static readonly Dictionary<string, Assembly> s_scripts = new(StringComparer.OrdinalIgnoreCase);

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

			// if type name is like .* then use script/module name as namespace
			if (res.TypeName.StartsWith('.'))
				res.TypeName = (res.ScriptName ?? res.ModuleName) + res.TypeName;
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

	static (Assembly, AssemblyLoadContext2?) LoadAssembly(ScriptParameters scriptParameters)
	{
		// load the module
		if (scriptParameters.ScriptName is null)
		{
			var manager = Far.Api.GetModuleManager(scriptParameters.ModuleName);
			return (manager.LoadAssembly(true), null);
		}

		// use permanently loaded script
		if (!scriptParameters.Unload && s_scripts.TryGetValue(scriptParameters.ScriptName, out Assembly? loadedAssembly))
		{
			return (loadedAssembly, null);
		}

		// load the script assembly
		var dll = $"{Environment.GetEnvironmentVariable("FARHOME")}\\FarNet\\Scripts\\{scriptParameters.ScriptName}\\{scriptParameters.ScriptName}.dll";
		var loader = new AssemblyLoadContext2(dll, scriptParameters.Unload);
		var assembly = loader.LoadFromAssemblyPath(dll);
		if (scriptParameters.Unload)
		{
			// to be unloaded
			return (assembly, loader);
		}
		else
		{
			// keep permanently loaded
			s_scripts.Add(scriptParameters.ScriptName, assembly);
			return (assembly, null);
		}
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
		var (assembly, loader) = LoadAssembly(scriptParameters);

		bool doFinallyComplete = true;
		void complete()
		{
			if (loader != null)
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
				var res = method.Invoke(instance, methodParameters);
				if (res is Task task)
				{
					doFinallyComplete = false;
					Task.Run(async () =>
					{
						try
						{
							await task;
						}
						catch (Exception ex)
						{
							await Tasks.Job(() =>
							{
								Far.Api.ShowError(null, ex);
							});
						}
						finally
						{
							complete();
						}
					});
				}
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException ?? ex;
			}
		}
		finally
		{
			if (doFinallyComplete)
				complete();
		}
	}
}
