using FarNet;
using System;
using System.IO;

namespace GitKit;

public class Host : ModuleHost
{
	public static Host Instance { get; private set; } = null!;

	static Lazy<Func<string, object[], object[]>?> s_invokeScriptArguments =
		new(() => (Func<string, object[], object[]>?)Far.Api.GetModuleInterop("PowerShellFar", "InvokeScriptArguments", null));

	public Host()
	{
		Instance = this;
	}

	static string? TryGitRoot(string? path)
	{
		if (string.IsNullOrEmpty(path))
			return null;

		var git = path + "/.git";
		if (Directory.Exists(git) || File.Exists(git))
			return path;

		return TryGitRoot(Path.GetDirectoryName(path));
	}

	public static string GetGitRoot(string path)
	{
		return TryGitRoot(path) ?? throw new ModuleException($"Not a git repository: {path}");
	}

	public static object[] InvokeScript(string script, object[] args)
	{
		var func = s_invokeScriptArguments.Value ?? throw new ModuleException("This operation requires FarNet.PowerShellFar");
		return func(script, args);
	}
}
