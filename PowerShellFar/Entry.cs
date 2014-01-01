
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

using System;
using System.Security.Permissions;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// INTERNAL
	/// </summary>
	[ModuleHost(Load = true)]
	public sealed class Entry : ModuleHost
	{
		/// <summary>
		/// INTERNAL
		/// </summary>
		internal static Entry Instance { get; private set; }
		///
		public Entry()
		{
			if (Instance != null)
				throw new InvalidOperationException();

			Instance = this;
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

			// connect actor
			A.Psf.Connect();
		}
		///
		[EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
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
			if (!InvokingHasBeenCalled)
			{
				A.Psf.Invoking();
				InvokingHasBeenCalled = true;
			}
		}
		bool InvokingHasBeenCalled;
		//! do not call Invoking(), it is done by FarNet
		internal static IModuleCommand CommandInvoke1 { get; private set; }
		void OnCommandInvoke1(object sender, ModuleCommandEventArgs e)
		{
			string currentDirectory = A.Psf.SyncPaths();
			try
			{
				A.Psf.Act(e.Command, new ConsoleOutputWriter(e.Command), !e.IsMacro);
			}
			finally
			{
				A.SetCurrentDirectoryFinally(currentDirectory);
			}
		}
		//! do not call Invoking(), it is done by FarNet
		internal static IModuleCommand CommandInvoke2 { get; private set; }
		void OnCommandInvoke2(object sender, ModuleCommandEventArgs e)
		{
			string currentDirectory = A.Psf.SyncPaths();
			try
			{
				A.Psf.Act(e.Command, null, !e.IsMacro);
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
		// "\s*prefix:\s*line" -> "\s*prefix:\s*" and "line"
		internal static void SplitCommandWithPrefix(ref string line, out string prefix)
		{
			string tmp = line.TrimStart();
			int delta = line.Length - tmp.Length;
			if (delta > 0)
			{
				prefix = line.Substring(0, delta);
				line = tmp;
			}
			else
			{
				prefix = string.Empty;
			}
			
			if (line.StartsWith((tmp = CommandInvoke1.Prefix + ":"), StringComparison.OrdinalIgnoreCase) ||
				line.StartsWith((tmp = CommandInvoke2.Prefix + ":"), StringComparison.OrdinalIgnoreCase))
			{
				prefix += line.Substring(0, tmp.Length);
				line = line.Substring(tmp.Length);
			}

			tmp = line.TrimStart();
			delta = line.Length - tmp.Length;
			if (delta > 0)
			{
				prefix += line.Substring(0, delta);
				line = tmp;
			}
		}
	}
}
