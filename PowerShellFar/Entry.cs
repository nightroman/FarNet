/*
PowerShellFar plugin for Far Manager
Copyright (C) 2006-2009 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Threading;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// FarNet entry point for internal use.
	/// </summary>
	public sealed class Entry : BasePlugin
	{
		static Entry _Instance;

		static String _Prefix1;
		internal static String Prefix1 { get { return _Prefix1; } }

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
				A.Far.Unregister(_Instance);
		}

		///
		public override void Connect()
		{
			// create an actor and expose main instances
			A.Connect(new Actor(), Far);

			// register prefixes
			_Prefix1 = Far.RegisterCommand(this, "PowerShell main prefix", ">", OnCommandLine);
			Far.RegisterCommand(this, "PowerShell jobs prefix", ">>", OnCommandLineJob);

			// register config
			Far.RegisterTool(this, Res.Name, OnConfig, ToolOptions.Config);

			// register disk
			Far.RegisterTool(this, "Power panel", OnDisk, ToolOptions.Disk);

			// register menu
			Far.RegisterTool(this, Res.Name, OnOpen, ToolOptions.F11Menus);

			// connect actor
			A.Psf.Connect();
		}

		///
		public override void Disconnect()
		{
			// detach tools
			Far.UnregisterCommand(OnCommandLine);
			Far.UnregisterCommand(OnCommandLineJob);
			Far.UnregisterTool(OnConfig);
			Far.UnregisterTool(OnDisk);
			Far.UnregisterTool(OnOpen);

			// disconnect instances
			A.Psf.Disconnect();
			A.Connect(null, null);
			_Instance = null;
		}

		///
		public override void Invoking()
		{
			A.Psf.Invoking();
		}

		void OnCommandLine(object sender, CommandEventArgs e)
		{
			//! do not call Invoking(), it is done by FarNet
			A.Psf.InvokePipeline(e.Command, null, true);
		}

		void OnCommandLineJob(object sender, CommandEventArgs e)
		{
			A.Psf.OnCommandLineJob(e.Command);
		}

		void OnConfig(object sender, ToolEventArgs e)
		{
			e.Ignore = !A.Psf.ShowSettings();
		}

		void OnDisk(object sender, ToolEventArgs e)
		{
			A.Psf.ShowPanel();
		}

		internal void OnOpen(object sender, ToolEventArgs e)
		{
			UI.ActorMenu.Show(sender, e);
		}

	}
}
