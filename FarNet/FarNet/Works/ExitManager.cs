namespace FarNet.Works;
#pragma warning disable 1591

public static class ExitManager
{
	static bool _exiting;
	static int _exitDelay;
	static Timer? _timerTimeout;
	static TaskCompletionSource<int>? _jobs;

	// Exiting means Far will exit after jobs.
	// Modules may alter work, especially errors.
	public static bool IsExiting { get; private set; }

	// Called by test runners before async jobs.
	public static void BeginJobs()
	{
		if (IsExiting)
		{
			if (_jobs is { })
				throw new InvalidOperationException("BeginJobs should be called once.");

			_jobs = Tasks.CreateAsyncTaskCompletionSource<int>();
		}
	}

	// Called by test runners after async jobs.
	public static void EndJobs()
	{
		if (IsExiting)
		{
			if (_jobs is null)
				throw new InvalidOperationException("BeginJobs should be called first.");

			_jobs.SetResult(0);
		}
	}

	// Called by the starter after jobs or modules on errors.
	public static void Exit(Exception? ex)
	{
		if (_exiting)
			return;

		_exiting = true;
		_ = Task.Run(async () =>
		{
			if (_jobs is { })
				await _jobs.Task;

			_timerTimeout?.Dispose();

			if (_exitDelay > 0)
				await Task.Delay(_exitDelay);

			if (ex is null)
			{
				/*
				NB Quit() works too, so it is possible but should be opt-in.
				It makes testing slower and has issues, e.g. odd, `-x <delay>`
				with small delay may "pause" and wait for any key to resume.
				*/

				// good exit: Exit(0)
				Far.Api.PostJob(() =>
				{
					Far.Api.UI.GetUserScreen(1);
					Environment.Exit(0);
				});
			}
			else
			{
				// fail exit: Exit(1)
				Log.TraceException(ex);
				Far.Api.PostJob(() =>
				{
					Far.Api.UI.GetUserScreen(1);
					Far.Api.UI.WriteLine($"Exit reason: {ex.Message}", ConsoleColor.Red);
					Environment.Exit(1);
				});
			}
		});
	}

	public static void SetDelay(int milliseconds)
	{
		IsExiting = true;
		_exitDelay = milliseconds;
	}

	public static void SetTimeout(int milliseconds)
	{
		IsExiting = true;
		if (milliseconds > 0)
			_timerTimeout ??= new Timer(Exit, null, milliseconds, Timeout.Infinite);

		void Exit(object? state)
		{
			Far.Api.UI.SetProgressState(TaskbarProgressBarState.NoProgress);
			var text = $"Exit timeout: {milliseconds}";
			Log.TraceError(text);
			Far.Api.UI.GetUserScreen(1);
			Far.Api.UI.WriteLine(text, ConsoleColor.Red);
			Environment.Exit(milliseconds);
		};
	}
}
