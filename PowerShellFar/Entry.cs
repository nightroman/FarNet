
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Management.Automation;

namespace PowerShellFar;

/// <summary>
/// INTERNAL
/// </summary>
[ModuleHost(Load = true)]
public sealed class Entry : ModuleHost
{
	internal static Entry Instance { get; private set; }
	internal static string LocalData { get; private set; }
	internal static string RoamingData { get; private set; }

	///
	public Entry()
	{
		if (Instance != null)
			throw new InvalidOperationException();

		Instance = this;
		LocalData = Manager.GetFolderPath(SpecialFolder.LocalData, true);
		RoamingData = Manager.GetFolderPath(SpecialFolder.RoamingData, true);
	}

	internal static void Unregister()
	{
		if (Instance != null)
			Instance.Manager.Unregister();
	}

	///
	public override void Connect()
	{
		// create an actor and expose main instances
		A.Connect(new Actor());

		// register commands with prefixes
		CommandInvoke1 = Manager.RegisterModuleCommand(
			new Guid("60353ab6-52cb-413e-8e11-e4917099b80b"),
			new ModuleCommandAttribute() { Name = "PowerShell command (console output)", Prefix = "ps" },
			OnCommandInvoke1);
		CommandInvoke2 = Manager.RegisterModuleCommand(
			new Guid("03760876-d154-467c-bc5d-8ec39efb637d"),
			new ModuleCommandAttribute() { Name = "PowerShell command (viewer output)", Prefix = "vps" },
			OnCommandInvoke2);

		// register menu
		Manager.RegisterModuleTool(
			new Guid("7def4106-570a-41ab-8ecb-40605339e6f7"),
			new ModuleToolAttribute() { Name = Res.Me, Options = ModuleToolOptions.F11Menus },
			OnOpen);

		// subscribe to editors
		Far.Api.AnyEditor.FirstOpening += EditorKit.OnEditorFirstOpening;
		Far.Api.AnyEditor.Opened += EditorKit.OnEditorOpened;

		// connect actor
		A.Psf.Connect();
	}

	///
	public override void Disconnect()
	{
		// disconnect instances
		A.Psf.Disconnect();
		A.Connect(null);
		Instance = null;
	}

	///
	public override bool CanExit()
	{
		return A.Psf.CanExit();
	}

	///
	public override void Invoking()
	{
		if (!IsInvokingCalled)
		{
			A.Psf.Invoking();
			IsInvokingCalled = true;
		}
	}
	bool IsInvokingCalled;

	internal static IModuleCommand CommandInvoke1 { get; private set; }

	void OnCommandInvoke1(object sender, ModuleCommandEventArgs e)
	{
		string currentDirectory = A.Psf.SyncPaths();
		try
		{
			// if command ends with `#` then omit history and echo else make echo with prefix
			var echo = e.Command.TrimEnd();
			var addHistory = !e.IsMacro;
			if (echo.EndsWith("#"))
			{
				addHistory = false;
				echo = null;
			}
			else
			{
				var colon = e.Command.Length > 0 && char.IsWhiteSpace(e.Command[0]) ? ":" : ": ";
				echo = CommandInvoke1.Prefix + colon + e.Command;
			}

			var ok = A.Psf.Run(new RunArgs(e.Command) { Writer = new ConsoleOutputWriter(echo), AddHistory = addHistory });
			e.Ignore = !ok;
		}
		finally
		{
			A.SetCurrentDirectoryFinally(currentDirectory);
		}
	}

	internal static IModuleCommand CommandInvoke2 { get; private set; }

	void OnCommandInvoke2(object sender, ModuleCommandEventArgs e)
	{
		string currentDirectory = A.Psf.SyncPaths();
		try
		{
			var ok = A.Psf.Run(new RunArgs(e.Command) { AddHistory = !e.IsMacro });
			e.Ignore = !ok;
		}
		finally
		{
			A.SetCurrentDirectoryFinally(currentDirectory);
		}
	}

	internal void OnOpen(object sender, ModuleToolEventArgs e)
	{
		UI.ActorMenu.Show(sender, e);
	}

	///
	public override object Interop(string command, object args)
	{
		return command switch
		{
			"InvokeScriptArguments" => new Func<string, object[], object[]>((string script, object[] arguments) =>
			{
				var result = A.InvokeCode(script, arguments);
				return PS2.UnwrapPSObject(result);
			}),

			"Runspace" => A.Psf.Runspace,

			_ => throw new ArgumentException("Unknown command.", nameof(command)),
		};
	}
}
