/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
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
		static Entry _Instance;

		internal static ModuleCommandAttribute Command1 { get; private set; }
		internal static ModuleCommandAttribute Command2 { get; private set; }

		///
		public Entry()
		{
			if (_Instance != null)
				throw new InvalidOperationException();

			_Instance = this;
		}

		internal static void Unregister()
		{
			if (_Instance != null)
				Far.Net.Unregister(_Instance);
		}

		///
		public override void Connect()
		{
			// create an actor and expose main instances
			A.Connect(new Actor());

			// register commands with prefixes
			{
				Command1 = new ModuleCommandAttribute();
				Command1.Name = "PowerShell main prefix";
				Command1.Prefix = ">";
				Far.Net.RegisterCommand(this.Manager, OnCommandLine, Command1);
			}
			{
				Command2 = new ModuleCommandAttribute();
				Command2.Name = "PowerShell jobs prefix";
				Command2.Prefix = ">>";
				Far.Net.RegisterCommand(this.Manager, OnCommandLineJob, Command2);
			}

			// register config
			{
				ModuleToolAttribute attr = new ModuleToolAttribute();
				attr.Name = Res.Me;
				attr.Options = ModuleToolOptions.Config;
				Far.Net.RegisterTool(this.Manager, OnConfig, attr);
			}

			// register menu
			{
				ModuleToolAttribute attr = new ModuleToolAttribute();
				attr.Name = Res.Me;
				attr.Options = ModuleToolOptions.F11Menus;
				Far.Net.RegisterTool(this.Manager, OnOpen, attr);
			}

			// editor events: OnEditorOpened1 should be called always and first
			// to do Invoking() (at least for TabExpansion) and the startup code
			Far.Net.AnyEditor.Opened += EditorKit.OnEditorOpened1;
			Far.Net.AnyEditor.Opened += EditorKit.OnEditorOpened2;

			// connect actor
			A.Psf.Connect();
		}

		///
		[EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
		public override void Disconnect()
		{
			// detach tools
			Far.Net.UnregisterCommand(OnCommandLine);
			Far.Net.UnregisterCommand(OnCommandLineJob);
			Far.Net.UnregisterTool(OnConfig);
			Far.Net.UnregisterTool(OnOpen);

			// disconnect instances
			A.Psf.Disconnect();
			A.Connect(null);
			_Instance = null;
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
		void OnCommandLine(object sender, ModuleCommandEventArgs e)
		{
			string currentDirectory = A.Psf.SyncPaths();
			try
			{
				A.Psf.InvokePipeline(e.Command, null, true);
			}
			finally
			{
				A.SetCurrentDirectoryFinally(currentDirectory);
			}
		}

		//! do not call Invoking(), it is done by FarNet
		//! do not sync paths for jobs
		void OnCommandLineJob(object sender, ModuleCommandEventArgs e)
		{
			string code = e.Command;
			Job job = new Job(new JobCommand(code, true), null, code, true, int.MaxValue);
			job.StartJob();
		}

		void OnConfig(object sender, ModuleToolEventArgs e)
		{
			e.Ignore = !A.Psf.ShowSettings();
		}

		internal void OnOpen(object sender, ModuleToolEventArgs e)
		{
			UI.ActorMenu.Show(sender, e);
		}

	}
}
