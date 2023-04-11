using FarNet;
using System;
using System.IO;

namespace GitKit;

public class Host : ModuleHost
{
	public static Host Instance { get; private set; } = null!;

	static readonly Lazy<Func<string, object[], object[]>?> s_invokeScriptArguments =
		new(() => (Func<string, object[], object[]>?)Far.Api.GetModuleInterop("PowerShellFar", "InvokeScriptArguments", null));

	public Host()
	{
		Instance = this;
	}

	public static string GetFullPath(string? path)
	{
		if (string.IsNullOrEmpty(path))
			return Far.Api.CurrentDirectory;

		path = Environment.ExpandEnvironmentVariables(path);
		return Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(Far.Api.CurrentDirectory, path));
	}

	public static object[] InvokeScript(string script, object[] args)
	{
		var func = s_invokeScriptArguments.Value ?? throw new ModuleException("This operation requires FarNet.PowerShellFar");
		return func(script, args);
	}
}
