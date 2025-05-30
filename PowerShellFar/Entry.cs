
using FarNet;
using System.Management.Automation;

namespace PowerShellFar;
#pragma warning disable 1591

/// <summary>
/// INTERNAL
/// </summary>
[ModuleHost(Load = true)]
public sealed class Entry : ModuleHost
{
	internal static Entry Instance { get; private set; } = null!;
	internal static string LocalData { get; private set; } = null!;
	internal static string RoamingData { get; private set; } = null!;
	internal static IModuleCommand CommandInvoke1 { get; private set; } = null!;
	internal static IModuleCommand CommandInvoke2 { get; private set; } = null!;

	internal static string Prefix1 { get; private set; } = null!;
	internal static string Prefix2 { get; private set; } = null!;

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
		Instance?.Manager.Unregister();
	}

	public override void Connect()
	{
		// create an actor and expose main instances
		A.Connect(new Actor());

		// register main command
		CommandInvoke1 = Manager.RegisterCommand(
			new ModuleCommandAttribute { Name = "PowerShell command, screen output", Prefix = "ps", Id = "60353ab6-52cb-413e-8e11-e4917099b80b" },
			OnCommandInvoke1);

		// register view command
		CommandInvoke2 = Manager.RegisterCommand(
			new ModuleCommandAttribute { Name = "PowerShell command, viewer output", Prefix = "vps", Id = "03760876-d154-467c-bc5d-8ec39efb637d" },
			OnCommandInvoke2);

		// register menu
		Manager.RegisterTool(
			new ModuleToolAttribute { Name = Res.Me, Options = ModuleToolOptions.F11Menus, Id = "7def4106-570a-41ab-8ecb-40605339e6f7" },
			(s, e) => UI.ActorMenu.Show(e));

		// prefixes
		Prefix1 = CommandInvoke1.Prefix + ':';
		Prefix2 = CommandInvoke2.Prefix + ':';

		// subscribe to editors
		Far.Api.AnyEditor.FirstOpening += EditorKit.OnEditorFirstOpening;
		Far.Api.AnyEditor.Opened += EditorKit.OnEditorOpened;

		// connect actor
		A.Psf.Connect();
	}

	public override void Disconnect()
	{
		// disconnect instances
		A.Psf.Disconnect();
		A.Connect(null);
		Instance = null!;
	}

	public override void Invoking()
	{
		if (!IsInvokingCalled)
		{
			A.Psf.Invoking();
			IsInvokingCalled = true;
		}
	}
	bool IsInvokingCalled;

	internal static bool IsMyPrefix(ReadOnlySpan<char> prefix)
	{
		return
			prefix.Equals(CommandInvoke1.Prefix, StringComparison.OrdinalIgnoreCase) ||
			prefix.Equals(CommandInvoke2.Prefix, StringComparison.OrdinalIgnoreCase);
	}

	void OnCommandInvoke1(object? sender, ModuleCommandEventArgs e)
	{
		var command = e.Command;

		// helper commands
		if (command.StartsWith('#'))
		{
			InvokeHelpers(command);
			return;
		}

		// Code
		A.Psf.SyncPaths();

		// echo / no echo
		Func<string>? getEcho;
		if (!command.StartsWith(' ') || command.EndsWith('#'))
			getEcho = null;
		else
			getEcho = () => CommandInvoke1.Prefix + ':' + command;

		var ok = A.Psf.Run(new RunArgs(command) { Writer = new ConsoleOutputWriter(getEcho) });
		e.Ignore = !ok;
	}

	void OnCommandInvoke2(object? sender, ModuleCommandEventArgs e)
	{
		A.Psf.SyncPaths();

		var ok = A.Psf.Run(new RunArgs(e.Command));
		e.Ignore = !ok;
	}

	public override object Interop(string command, object? args)
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

	static void InvokeHelpers(ReadOnlySpan<char> command)
	{
		switch (command.TrimEnd())
		{
			case "#invoke":
				EditorKit.InvokeSelectedCode();
				return;
			case "#complete":
				EditorKit.ExpandCode(null, null);
				return;
			case "#history":
				A.Psf.ShowHistory();
				return;
			case "#enter":
				EditorKit.PlayNativeEnter();
				return;
			case "#line-breakpoint":
				DebuggerKit.OnLineBreakpoint();
				return;
			default:
				throw new ModuleException($"Invalid command: '{command}'.");
		}
	}
}
