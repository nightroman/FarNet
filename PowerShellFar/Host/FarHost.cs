using FarNet;
using System.Globalization;
using System.Management.Automation.Host;

namespace PowerShellFar;

// Host implementation.
class FarHost : PSHost
{
	// Original current culture.
	readonly CultureInfo _CurrentCulture = Thread.CurrentThread.CurrentCulture;

	// Injected UI.
	readonly PSHostUserInterface _UI;

	// Current nested prompt.
	IEditor? _nestedPromptEditor;

	// Construct an instance of this PSHost implementation.
	// Keep a reference to the hosting application object.
	internal FarHost(PSHostUserInterface ui)
	{
		_UI = ui;
	}

	internal static void Init()
	{
		s_ignoreApplications = false;
	}

	#region PSHost
	// The host name.
	public override string Name => "FarHost";

	// Gets the current culture to use.
	public override CultureInfo CurrentCulture => _CurrentCulture;

	// Gets the current UI culture to use.
	public override CultureInfo CurrentUICulture => A.Psf.Manager.CurrentUICulture;

	// Gets the ID.
	public override Guid InstanceId { get; } = Guid.NewGuid();

	// Gets the UI instance.
	public override PSHostUserInterface UI => _UI;

	// Gets the assembly version.
	public override Version Version => typeof(Actor).Assembly.GetName().Version!;

	// Instructs the host to interrupt the currently running pipeline and start a new nested input loop.
	// An input loop is the cycle of prompt, input, and execute.
	public override void EnterNestedPrompt()
	{
		// push the last
		IEditor? keepNested = _nestedPromptEditor;

		try
		{
			//! Far used to crash: Test-CallStack.ps1 \ suspend \ type exit + enter
			//! This exception from Open() was removed, so don't try\catch all in here.
			//! SVN tag 4.2.26
			_nestedPromptEditor = Interactive.Create(true).Editor;

			// Enter the modal editor. There are two ways to exit.
			// 1) User exits the editor ([Esc]/[F10]). _nested should be this editor, not null.
			// But PowerShell nested prompt is not yet exited, call 'exit', it triggers
			// ExitNestedPrompt(), it sets _nested to null.
			// 2) User types 'exit' in the editor. Then ExitNestedPrompt() is called first,
			// it sets _nested to null and closes the editor. Control gets here with null
			// _nested, so we do nothing but restoring the very first _nested.
			_nestedPromptEditor.Open(OpenMode.Modal);

			// If _nested is not null then a user has closed the editor via UI, not by 'exit'.
			// Thus, we have to exit the nested prompt. IsRunning check is added for V3 CTP2.
			// It works fine in V2, too. Meaning: if there is no running pipeline (stepper)
			// then there is nothing to exit, so do not exit. Exit nothing hangs in V3 CTP2.
			if (_nestedPromptEditor != null && A.Psf.IsRunning)
			{
				using var ps = A.Psf.NewPowerShell();
				ps.AddScript("exit").Invoke();
			}
		}
		finally
		{
			// pop the last
			_nestedPromptEditor = keepNested;
		}
	}

	// Instructs the host to exit the currently running input loop.
	public override void ExitNestedPrompt()
	{
		if (_nestedPromptEditor != null)
		{
			var nested = _nestedPromptEditor;
			_nestedPromptEditor = null;
			if (nested.IsOpened)
				nested.Close();
		}
	}

	// Called before an external application process is started.
	// It is used to save state that the child process may alter
	// so the parent can restore that state when the child exits.
	public override void NotifyBeginApplication()
	{
		if (!s_ignoreApplications)
			Far.Api.UI.ShowUserScreen();
	}

	// Called after an external application process finishes.
	// It is used to restore state that the child process may have altered.
	public override void NotifyEndApplication()
	{
		if (!s_ignoreApplications)
			Far.Api.UI.SaveUserScreen();
	}

	// Indicates to the host that an exit has been requested.
	// It passes the exit code that the host should use when exiting the process.
	public override void SetShouldExit(int exitCode)
	{
	}
	#endregion

	#region IgnoreApplications
	//! Default is true for profile loading.
	static bool s_ignoreApplications = true;

	// Use with `using` to disable/restore Notify*Application().
	internal sealed class IgnoreApplications : IDisposable
	{
		readonly bool _old = s_ignoreApplications;

		public IgnoreApplications()
		{
			s_ignoreApplications = true;
		}

		public void Dispose()
		{
			s_ignoreApplications = _old;
		}
	}
	#endregion
}
