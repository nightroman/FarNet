using FarNet;
using System;
using System.IO;

namespace GitKit;

public class Host : ModuleHost
{
	public static Host Instance { get; private set; } = null!;

	static Func<string, object[], object[]>? s_psInvokeScriptArguments;

	public Host()
	{
		Instance = this;
	}

	public static object[] InvokeScript(string script, object[] args)
	{
		if (s_psInvokeScriptArguments is null)
		{
			try
			{
				var moduleManager = Far.Api.GetModuleManager("PowerShellFar");
				s_psInvokeScriptArguments = (Func<string, object[], object[]>)moduleManager.Interop("InvokeScriptArguments", null!);
			}
			catch
			{
				s_psInvokeScriptArguments = (s, a) => throw new ModuleException("This operation requires FarNet.PowerShellFar");
			}
		}

		return s_psInvokeScriptArguments(script, args);
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
}
