
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
*/

using System;
using System.Security.Permissions;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// FarNet module host for internal use.
	/// </summary>
	[ModuleHost(Load = true)]
	public sealed class Entry : ModuleHost
	{
		/// <summary>
		/// For internal use.
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
				new ModuleCommandAttribute() { Name = "PowerShell command (console output)", Prefix = ">" },
				OnCommandInvoke1);
			CommandInvoke2 = Manager.RegisterModuleCommand(
				new Guid("03760876-d154-467c-bc5d-8ec39efb637d"),
				new ModuleCommandAttribute() { Name = "PowerShell command (viewer output)", Prefix = ">>" },
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
				A.Psf.Act(e.Command, new ConsoleOutputWriter(e.Command), e.MacroArea == MacroArea.None);
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
				A.Psf.Act(e.Command, null, e.MacroArea == MacroArea.None);
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
	}
}
