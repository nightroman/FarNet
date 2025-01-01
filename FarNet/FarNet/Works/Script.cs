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
		public string? ModuleName;
		public string? ScriptName;
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

	static ScriptParameters ParseScriptParameters(CommandParameters parameters)
	{
		// script and module
		var res = new ScriptParameters
		{
			ScriptName = parameters.GetString(KeyScript),
			ModuleName = parameters.GetString(KeyModule)
		};

		if (res.ScriptName is null && res.ModuleName is null)
			throw new ModuleException($"Missing required parameter '{KeyScript}' or '{KeyModule}'.");

		if (res.ScriptName is { } && res.ModuleName is { })
			throw new ModuleException($"Parameters '{KeyScript}' and '{KeyModule}' cannot be used together.");

		// method
		var method = parameters.GetRequiredString(KeyMethod);
		{
			int index = method.LastIndexOf('.');
			if (index < 0)
				throw parameters.ParameterError(KeyMethod, "Invalid method name, expected dot notation.");

			res.TypeName = method[..index];
			res.MethodName = method[(index + 1)..];

			// dot shortcut, use script/module name as namespace
			if (res.TypeName.StartsWith('.'))
				res.TypeName = (res.ScriptName ?? res.ModuleName) + res.TypeName;
		}

		// unload
		res.Unload = parameters.GetBool(KeyUnload);

		// assert
		parameters.ThrowUnknownParameters();

		return res;
	}

	static object?[]? ParseMethodParameters(MethodInfo method, ReadOnlySpan<char> text)
	{
		var methodParameters = method.GetParameters();
		if (methodParameters.Length == 0)
		{
			if (text.Length > 0)
				throw new InvalidOperationException("Method does not have parameters.");

			return null;
		}

		var sb = text.Length == 0 ? null : CommandParameters.ParseParameters(text.ToString());

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
		if (scriptParameters.ModuleName is { })
		{
			var manager = Far.Api.GetModuleManager(scriptParameters.ModuleName);
			return (manager.LoadAssembly(true), null);
		}

		// use permanently loaded script
		string scriptName = scriptParameters.ScriptName!;
		if (!scriptParameters.Unload && s_scripts.TryGetValue(scriptName, out Assembly? loadedAssembly))
		{
			return (loadedAssembly, null);
		}

		// load the script assembly
		var dll = $"{Environment.GetEnvironmentVariable("FARHOME")}\\FarNet\\Scripts\\{scriptName}\\{scriptName}.dll";
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
			s_scripts.Add(scriptName, assembly);
			return (assembly, null);
		}
	}

	public static void InvokeScript(string command)
	{
		// get script parameters with separated text
		var parameters = CommandParameters.Parse(command, false, "::");
		var scriptParameters = ParseScriptParameters(parameters);

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
			var type = assembly.GetType(scriptParameters.TypeName)
				?? throw new Exception($"Cannot find type '{scriptParameters.TypeName}'.");

			// find method
			var method = type.GetMethod(scriptParameters.MethodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
				?? throw new Exception($"Cannot find method '{scriptParameters.MethodName}'.");

			// parse method parameters
			var methodParameters = ParseMethodParameters(method, parameters.Text);

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
