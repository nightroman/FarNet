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

			// ref: 2025-11-28-0602
			//Far.Api.UI.GetUserScreen(1); //rk-0
		}
	}

	// Called by test runners after async jobs.
	public static void EndJobs()
	{
		if (IsExiting)
		{
			if (_jobs is null)
				throw new InvalidOperationException("BeginJobs should be called first.");

			//! pwsf -x 9 Test-FarNet.ps1
			Far.Api.UI.GetUserScreen(1); //rk-0

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

			Console.ResetColor();

			if (ex is null)
				Environment.Exit(0);

			Console.WriteLine($"\nExit reason: {ex.Message}");

			Log.TraceException(ex);
			Environment.Exit(1);
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
		{
			_timerTimeout ??= new Timer(s =>
				{
					var text = $"Exit timeout: {milliseconds}";

					Console.WriteLine();
					Console.WriteLine(text);

					Log.TraceError(text);
					Environment.Exit(milliseconds);
				},
				null,
				milliseconds,
				Timeout.Infinite);
		}
	}
}
