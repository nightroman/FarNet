
namespace FarNet.Works;
#pragma warning disable 1591

public static class Test
{
	// Set by SetTest, usually by Start-Far.
	// Used by command runners for altering their work.
	public static bool IsTestCommand { get; private set; }

	static int _exitDelay;
	static Timer? _timerTimeout;

	// Used by the command runner code.
	public static void Exit(Exception ex)
	{
		_timerTimeout?.Dispose();

		// exit immediately
		if (_exitDelay <= 0)
			exit();

		// exit after delay
		_ = Task.Run(async () =>
		{
			await Task.Delay(_exitDelay);
			exit();
		});

		void exit()
		{
			if (ex is null)
				Environment.Exit(0);

			Log.TraceException(ex);
			Environment.Exit(1);
		}
	}

	// Used by Start-Far.ps1
	public static void SetTest(int milliseconds)
	{
		IsTestCommand = true;
		_exitDelay = milliseconds;
	}

	// Used by Start-Far.ps1
	public static void SetTimeout(int milliseconds)
	{
		if (milliseconds > 0)
		{
			_timerTimeout ??= new Timer(s =>
				{
					Log.TraceError("Timeout exit.");
					Environment.Exit(milliseconds);
				},
				null,
				milliseconds,
				Timeout.Infinite);
		}
	}

	static void AssertNormalPanel(IPanel? panel, string active)
	{
		if (panel is null)
			throw new InvalidOperationException("Expected panel.");

		if (panel.IsPlugin)
			throw new InvalidOperationException($"Expected {active} panel type: Native, actual: Plugin.");

		if (panel.Kind != PanelKind.File)
			throw new InvalidOperationException($"Expected {active} panel kind: File, actual: {panel.Kind}.");

		if (!panel.IsVisible)
			throw new InvalidOperationException($"Expected {active} panel state: Visible, actial: Hidden.");

		if (panel.SelectedFirst)
			throw new InvalidOperationException($"Expected {active} panel SelectedFirst: Off, actial: On.");
	}

	public static void AssertNormalState()
	{
		//! test kind first
		if (Far.Api.Window.Kind != WindowKind.Panels)
			throw new InvalidOperationException($"Expected window: Panels, actual: {Far.Api.Window.Kind}.");

		// 2 = Panels + Desktop
		if (Far.Api.Window.Count != 2)
			throw new InvalidOperationException("Expected no windows but Panels.");

		AssertNormalPanel(Far.Api.Panel, "active");
		AssertNormalPanel(Far.Api.Panel2, "passive");
	}
}
