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
	/// FarNet module for internal use.
	/// </summary>
	public sealed class Entry : BaseModule
	{
		static Entry _Instance;

		internal static String Prefix1 { get; private set; }
		internal static String Prefix2 { get; private set; }

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
				Far.Host.Unregister(_Instance);
		}

		///
		public override void Connect()
		{
			// create an actor and expose main instances
			A.Connect(new Actor());

			// register prefixes
			Prefix1 = Far.Host.RegisterCommand(this, "PowerShell main prefix", ">", OnCommandLine);
			Prefix2 = Far.Host.RegisterCommand(this, "PowerShell jobs prefix", ">>", OnCommandLineJob);

			// register config
			Far.Host.RegisterTool(this, Res.Me, OnConfig, ToolOptions.Config);

			// register menu
			Far.Host.RegisterTool(this, Res.Me, OnOpen, ToolOptions.F11Menus);

			// editor events: OnEditorOpened1 should be called always and first
			// to do Invoking() (at least for TabExpansion) and the startup code
			Far.Host.AnyEditor.Opened += EditorKit.OnEditorOpened1;
			Far.Host.AnyEditor.Opened += EditorKit.OnEditorOpened2;

			// connect actor
			A.Psf.Connect();
		}

		///
		[EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
		public override void Disconnect()
		{
			// detach tools
			Far.Host.UnregisterCommand(OnCommandLine);
			Far.Host.UnregisterCommand(OnCommandLineJob);
			Far.Host.UnregisterTool(OnConfig);
			Far.Host.UnregisterTool(OnOpen);

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
		void OnCommandLine(object sender, CommandEventArgs e)
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
		void OnCommandLineJob(object sender, CommandEventArgs e)
		{
			string code = e.Command;
			Job job = new Job(new JobCommand(code, true), null, code, true, int.MaxValue);
			job.StartJob();
		}

		void OnConfig(object sender, ToolEventArgs e)
		{
			e.Ignore = !A.Psf.ShowSettings();
		}

		internal void OnOpen(object sender, ToolEventArgs e)
		{
			UI.ActorMenu.Show(sender, e);
		}

	}
}
