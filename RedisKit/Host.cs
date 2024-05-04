using FarNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace RedisKit;

public class Host : ModuleHost
{
	public const string MyName = "RedisKit";
	public const string RedisKit_User = "RedisKit_User";
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

		return Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(Far.Api.CurrentDirectory, path));
	}

	public static object[] InvokeScript(string script, object[] args)
	{
		var func = s_invokeScriptArguments.Value ?? throw new ModuleException("This operation requires FarNet.PowerShellFar");
		return func(script, args);
	}

	static void UpdatePanel(IPanel? panel)
	{
		if (panel is AnyPanel)
		{
			panel.Update(true);
			panel.Redraw();
		}
	}

	public static void UpdatePanels()
	{
		UpdatePanel(Far.Api.Panel);
		UpdatePanel(Far.Api.Panel2);
	}
}
