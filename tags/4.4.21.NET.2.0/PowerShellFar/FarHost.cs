
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Reflection;
using System.Threading;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// PowerShellFar host implementation.
	/// </summary>
	class FarHost : PSHost
	{
		// UI object
		PSHostUserInterface _UI;

		/// <summary>
		/// Construct an instance of this PSHost implementation.
		/// Keep a reference to the hosting application object.
		/// </summary>
		/// <param name="ui">Host UI.</param>
		internal FarHost(PSHostUserInterface ui)
		{
			_UI = ui;
		}

		#region PSHost

		/// <summary>
		/// The host name: FarHost
		/// </summary>
		public override string Name
		{
			get { return "FarHost"; }
		}

		/// <summary>
		/// Gets the current culture to use.
		/// </summary>
		public override CultureInfo CurrentCulture
		{
			get { return _CurrentCulture; }
		}
		CultureInfo _CurrentCulture = Thread.CurrentThread.CurrentCulture;

		/// <summary>
		/// Gets the current UI culture to use.
		/// </summary>
		public override CultureInfo CurrentUICulture
		{
			get { return A.Psf.Manager.CurrentUICulture; }
		}

		/// <summary>
		/// Gets the GUID generated once.
		/// </summary>
		public override Guid InstanceId
		{
			get { return _InstanceId; }
		}
		static Guid _InstanceId = Guid.NewGuid();

		/// <summary>
		/// Gets the UI instance.
		/// </summary>
		public override PSHostUserInterface UI
		{
			get { return _UI; }
		}

		/// <summary>
		/// Gets the assembly version.
		/// </summary>
		public override Version Version
		{
			get { return Assembly.GetExecutingAssembly().GetName().Version; }
		}

		/// <summary>
		/// Instructs the host to interrupt the currently running pipeline and start a new nested input loop.
		/// An input loop is the cycle of prompt, input, and execute.
		/// </summary>
		public override void EnterNestedPrompt()
		{
			// push the last
			IEditor keepNested = _nested;

			try
			{
				//! Far used to crash: Test-CallStack-.ps1 \ suspend \ type exit + enter
				//! This exception from Open() was removed, so don't try\catch all in here.
				//! SVN tag 4.2.26
				EditorConsole console = EditorConsole.CreateConsole(false);
				_nested = console.Editor;
				_nested.Open(OpenMode.Modal);

				// it is exited
				if (_nested != null)
				{
					//! case editor closed: do exit; DON'T use script block
					using (PowerShell pipe = A.Psf.CreatePipeline())
					{
						pipe.Commands.AddScript("exit");
						pipe.Invoke();
					}
				}
			}
			finally
			{
				// pop the last
				_nested = keepNested;
			}
		}
		IEditor _nested;

		/// <summary>
		/// Instructs the host to exit the currently running input loop.
		/// </summary>
		public override void ExitNestedPrompt()
		{
			if (_nested != null)
			{
				if (_nested.IsOpened)
					_nested.Close();
				_nested = null;
			}
		}

		/// <summary>
		/// Called before an external application process is started.
		/// It is used to save state that the child process may alter
		/// so the parent can restore that state when the child exits.
		/// </summary>
		public override void NotifyBeginApplication()
		{
			// PSF 2.2.10 Do not need this.
		}

		/// <summary>
		/// Called after an external application process finishes.
		/// It is used to restore state that the child process may have altered.
		/// </summary>
		public override void NotifyEndApplication()
		{
			// PSF 2.2.10 Do not need this.
		}

		/// <summary>
		/// Indicates to the host that an exit has been requested.
		/// It passes the exit code that the host should use when exiting the process.
		/// </summary>
		public override void SetShouldExit(int exitCode)
		{
		}

		#endregion
	}
}
