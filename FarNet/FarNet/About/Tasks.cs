
using FarNet.Forms;
using System.Diagnostics;

namespace FarNet;

/// <summary>
/// Helpers for tasks and jobs.
/// Jobs are actions and functions called by the core when it gets control.
/// </summary>
public static class Tasks
{
	/// <summary>
	/// Creates it with asynchronous continuations.
	/// </summary>
	/// <typeparam name="T">Task result type.</typeparam>
	/// <returns>.</returns>
	public static TaskCompletionSource<T> CreateAsyncTaskCompletionSource<T>()
	{
		return new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
	}

	/// <summary>
	/// Creates it with synchronous continuations.
	/// </summary>
	/// <typeparam name="T">Task result type.</typeparam>
	/// <returns>.</returns>
	public static TaskCompletionSource<T> CreateSyncTaskCompletionSource<T>()
	{
#pragma warning disable EPC32
		return new TaskCompletionSource<T>();
#pragma warning restore EPC32
	}

	/// <summary>
	/// In special cases, waits for the task completion.
	/// </summary>
	/// <param name="task">The task.</param>
	public static void Await(this Task task)
	{
		task.GetAwaiter().GetResult();
	}

	/// <summary>
	/// In special cases, waits for the task result.
	/// </summary>
	/// <typeparam name="T">The task result type.</typeparam>
	/// <param name="task">The task.</param>
	/// <returns>The result.</returns>
	public static T AwaitResult<T>(this Task<T> task)
	{
		return task.GetAwaiter().GetResult();
	}

	/// <summary>
	/// Starts a task with the specified function job.
	/// </summary>
	/// <typeparam name="T">The function result.</typeparam>
	/// <param name="job">The function job.</param>
	/// <returns>The task which completes when the function job completes.</returns>
	public static Task<T> Job<T>(Func<T> job)
	{
		var tcs = CreateSyncTaskCompletionSource<T>();

		Far.Api.PostJob(() =>
		{
			try
			{
				tcs.SetResult(job());
			}
			catch (Exception ex)
			{
				tcs.SetException(ex);
			}
		});

		return tcs.Task;
	}

	/// <summary>
	/// Starts a task with the specified action job.
	/// </summary>
	/// <param name="job">The action job.</param>
	/// <returns>The task which completes when the action job completes.</returns>
	/// <remarks>
	/// If the job opens a panel, use <see cref="OpenPanel(Action)"/> instead.
	/// If the job may open a panel, use <see cref="Command"/> instead.
	/// </remarks>
	public static Task Job(Action job)
	{
		var tcs = CreateSyncTaskCompletionSource<object>();

		Far.Api.PostJob(() =>
		{
			try
			{
				job();
				tcs.SetResult(null!);
			}
			catch (Exception ex)
			{
				tcs.SetException(ex);
			}
		});

		return tcs.Task;
	}

	/// <summary>
	/// Starts a task which runs the job and completes when the core gets control.
	/// </summary>
	/// <param name="job">The job to run.</param>
	/// <returns>The task which completes when the core gets control.</returns>
	/// <remarks>
	/// The task completes when the core gets control,
	/// either on shown UI or the job end, whatever happens first.
	/// <para>
	/// The job is supposed to show some modal UI and
	/// let next tasks work for automation, tests, etc.
	/// </para>
	/// </remarks>
	public static Task Run(Action job)
	{
		var tcs = CreateSyncTaskCompletionSource<object>();

		Far.Api.PostJob(() =>
		{
			try
			{
				//! try because the job may fail before UI
				Far.Api.PostJob(() => tcs.TrySetResult(null!));
				job();
			}
			catch (Exception ex)
			{
				//! try because the task may complete on UI and the job may fail after
				if (!tcs.TrySetException(ex))
					throw;
			}
		});

		return tcs.Task;
	}

	/// <summary>
	/// Awaits the task and processes exceptions in the main thread.
	/// </summary>
	/// <param name="task">The task function.</param>
	/// <param name="error">The optional exception handler.</param>
	public static async Task ExecuteAndCatch(Func<Task> task, Action<Exception>? error = null)
	{
		try
		{
			await task();
		}
		catch (Exception ex)
		{
			await Job(() =>
			{
				if (error is null)
					Far.Api.ShowError(null, ex);
				else
					error(ex);
			});
		}
	}

	/// <summary>
	/// Starts a task which posts the specified keys.
	/// </summary>
	/// <param name="keys">Keys text to post.</param>
	/// <returns>The task which completes when the keys complete.</returns>
	public static Task Keys(string keys)
	{
		return Macro($"Keys[[{keys}]]");
	}

	/// <summary>
	/// Starts a task which posts the specified macro.
	/// </summary>
	/// <param name="text">Macro text to post.</param>
	/// <returns>The task which completes when the macro completes.</returns>
	public static Task Macro(string text)
	{
		var tcs = CreateSyncTaskCompletionSource<object>();

		Far.Api.PostJob(() =>
		{
			try
			{
				var wait = Works.Far2.Api.PostMacroWait(text);
				ThreadPool.QueueUserWorkItem(_ =>
				{
					wait.WaitOne();
					wait.Dispose();
					tcs.SetResult(null!);
				});
			}
			catch (Exception ex)
			{
				tcs.SetException(ex);
				return;
			}
		});

		return tcs.Task;
	}

	/// <summary>
	/// Starts a task which opens the editor if not yet and completes when it closes.
	/// </summary>
	/// <param name="editor">The editor to open.</param>
	/// <returns>The task which completes when the editor closes.</returns>
	public static Task Editor(IEditor editor)
	{
		var tcs = CreateSyncTaskCompletionSource<object>();

		void onClosed(object? sender, EventArgs e)
		{
			editor.Closed -= onClosed;
			tcs.SetResult(null!);
		}

		Far.Api.PostJob(() =>
		{
			try
			{
				editor.Closed += onClosed;
				if (!editor.IsOpened)
					editor.Open();
			}
			catch (Exception ex)
			{
				editor.Closed -= onClosed;
				tcs.SetException(ex);
			}
		});

		return tcs.Task;
	}

	/// <summary>
	/// Starts a task which opens the viewer if not yet and completes when it closes.
	/// </summary>
	/// <param name="viewer">The viewer to open.</param>
	/// <returns>The task which completes when the viewer closes.</returns>
	public static Task Viewer(IViewer viewer)
	{
		var tcs = CreateSyncTaskCompletionSource<object>();

		void onClosed(object? sender, EventArgs e)
		{
			viewer.Closed -= onClosed;
			tcs.SetResult(null!);
		}

		Far.Api.PostJob(() =>
		{
			try
			{
				viewer.Closed += onClosed;
				if (!viewer.IsOpened)
					viewer.Open();
			}
			catch (Exception ex)
			{
				viewer.Closed -= onClosed;
				tcs.SetException(ex);
			}
		});

		return tcs.Task;
	}

	/// <summary>
	/// Starts a task which opens the dialog and completes when it closes.
	/// </summary>
	/// <param name="dialog">The dialog to open.</param>
	/// <returns>The task which completes when the dialog closes.</returns>
	public static Task Dialog(IDialog dialog)
	{
		var tcs = CreateSyncTaskCompletionSource<object>();

		void onClosed(object? sender, AnyEventArgs e)
		{
			dialog.Closed -= onClosed;
			tcs.SetResult(null!);
		}

		Far.Api.PostJob(() =>
		{
			try
			{
				dialog.Closed += onClosed;
				dialog.Open();
			}
			catch (Exception ex)
			{
				dialog.Closed -= onClosed;
				tcs.SetException(ex);
			}
		});

		return tcs.Task;
	}

	/// <summary>
	/// Starts a task which opens the dialog with the specified closing function.
	/// </summary>
	/// <typeparam name="T">The closing result.</typeparam>
	/// <param name="dialog">The dialog to open.</param>
	/// <param name="closing">The closing function.</param>
	/// <returns>The task which completes when the dialog closes.</returns>
	/// <remarks>
	/// The closing function is similar to <see cref="IDialog.Closing"/> handlers but with a result.
	/// The result should take cancel into account (<see cref="AnyEventArgs.Control"/> is null).
	/// For example null on cancel and not null otherwise.
	/// </remarks>
	public static Task<T> Dialog<T>(IDialog dialog, Func<ClosingEventArgs, T> closing)
	{
		var tcs = CreateSyncTaskCompletionSource<T>();

		void onClosing(object? sender, ClosingEventArgs e)
		{
			try
			{
				var res = closing(e);
				if (e.Ignore)
					return;

				dialog.Closing -= onClosing;
				tcs.SetResult(res);
			}
			catch (Exception ex)
			{
				dialog.Closing -= onClosing;
				tcs.SetException(ex);
			}
		}

		Far.Api.PostJob(() =>
		{
			try
			{
				dialog.Closing += onClosing;
				dialog.Open();
			}
			catch (Exception ex)
			{
				dialog.Closing -= onClosing;
				tcs.SetException(ex);
			}
		});

		return tcs.Task;
	}

	/// <summary>
	/// Starts a task which opens the specified panel.
	/// </summary>
	/// <param name="panel">The panel to open.</param>
	/// <returns>The task which completes when the panel is opened.</returns>
	public static async Task OpenPanel(Panel panel)
	{
		// open
		await Job(panel.Open);

		// wait
		await Works.Far2.Api.WaitSteps();

		// test
		await Job(() =>
		{
			if (Far.Api.Panel != panel)
				throw new InvalidOperationException("Panel was not opened.");
		});
	}

	/// <summary>
	/// Starts a task which calls the specified job which opens a panel.
	/// </summary>
	/// <param name="job">The job to open a panel.</param>
	/// <returns>The task which completes when a panel is opened. Its result is the opened panel.</returns>
	public static async Task<Panel> OpenPanel(Action job)
	{
		// open
		var oldPanel = await Job(() =>
		{
			var panel = Far.Api.Panel;
			job();
			return panel;
		});

		// wait
		await Works.Far2.Api.WaitSteps();

		// test and return
		return await Job(() =>
		{
			if (Far.Api.Panel is Panel newPanel && newPanel != oldPanel)
				return newPanel;

			throw new InvalidOperationException("Panel was not opened.");
		});
	}

	/// <summary>
	/// Starts a task with the command like job.
	/// </summary>
	/// <param name="job">The command like job.</param>
	/// <returns>The task which awaits the job and some internal extras.</returns>
	/// <remarks>
	/// If the job does not open a panel, use <see cref="Job"/> instead.
	/// If the job opens a panel, use <see cref="OpenPanel(Action)"/> instead.
	/// This method is for uncertain jobs like invoking interactive commands (REPL).
	/// </remarks>
	public static async Task<object?> Command(Action job)
	{
		// open
		var oldPanel = await Job(() =>
		{
			var panel = Far.Api.Panel;
			job();
			return panel;
		});

		// wait
		await Works.Far2.Api.WaitSteps();

		// test and return
		return await Job(() =>
		{
			if (Far.Api.Panel is Panel newPanel && newPanel != oldPanel)
				return newPanel;

			return null;
		});
	}

	/// <summary>
	/// Starts a task which waits until the specified panel is closed.
	/// </summary>
	/// <param name="panel">The panel.</param>
	/// <returns>The task which completes when the panel is closed.</returns>
	public static Task WaitPanelClosed(Panel panel)
	{
		var tcs = CreateSyncTaskCompletionSource<object>();

		//! post to avoid race for IsOpened
		Far.Api.PostJob(() =>
		{
			if (!panel.IsOpened)
			{
				tcs.SetResult(null!);
				return;
			}

			void onClosed(object? sender, EventArgs e)
			{
				panel.Closed -= onClosed;
				tcs.SetResult(null!);
			}

			panel.Closed += onClosed;
		});
		return tcs.Task;
	}

	/// <summary>
	/// Starts a task which waits until the specified panel is closed with the closing job.
	/// </summary>
	/// <typeparam name="T">Type of the closing job result.</typeparam>
	/// <param name="panel">The panel.</param>
	/// <param name="closing">The closing job, similar to the closing event handler but with the result.</param>
	/// <returns>The task which completes when the panel is closed and gets the closing result.</returns>
	public static Task<T> WaitPanelClosing<T>(Panel panel, Func<PanelEventArgs, T> closing)
	{
		var tcs = CreateSyncTaskCompletionSource<T>();

		//! post to avoid race for IsOpened
		Far.Api.PostJob(() =>
		{
			var result = default(T);

			if (!panel.IsOpened)
			{
				tcs.SetResult(result!);
				return;
			}

			void onClosing(object? sender, PanelEventArgs e)
			{
				try
				{
					var r = closing(e);
					if (!e.Ignore)
						result = r;
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
				}
			}

			void onClosed(object? sender, EventArgs e)
			{
				panel.Closing -= onClosing;
				panel.Closed -= onClosed;
				tcs.TrySetResult(result!);
			}

			panel.Closing += onClosing;
			panel.Closed += onClosed;
		});
		return tcs.Task;
	}

	/// <summary>
	/// Starts a task which opens the specified panel and waits until it is closed.
	/// </summary>
	/// <param name="panel">The panel.</param>
	/// <returns>The task which completes when the panel is closed.</returns>
	public static async Task Panel(Panel panel)
	{
		await OpenPanel(panel);
		await WaitPanelClosed(panel);
	}

	/// <summary>
	/// Starts a task which waits until the job returns true.
	/// </summary>
	/// <param name="delay">Milliseconds to delay when the predicate returns false.</param>
	/// <param name="timeout">Maximum waiting time in milliseconds. Use 0 for infinite.</param>
	/// <param name="job">Returns true to stop waiting.</param>
	/// <returns>True if the job returns true before the time is out.</returns>
	public static async Task<bool> Wait(int delay, int timeout, Func<bool> job)
	{
		var span = timeout > 0 ? TimeSpan.FromMilliseconds(timeout) : TimeSpan.MaxValue;
		var time = Stopwatch.StartNew();
		for (; ; )
		{
			var ok = await Job(job);
			if (ok)
				return true;

			if (time.Elapsed > span)
				return false;

			await Task.Delay(delay);
		}
	}
}
