
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet.Forms;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FarNet;

/// <summary>
/// Async helpers for jobs.
/// Jobs are actions and functions called by the core when it gets control.
/// </summary>
public static class Tasks
{
	/// <summary>
	/// Starts a task with the specified function job.
	/// </summary>
	/// <typeparam name="T">The function result.</typeparam>
	/// <param name="job">The function job.</param>
	/// <returns>The task which completes when the function job completes.</returns>
	public static Task<T> Job<T>(Func<T> job)
	{
		var task = new TaskCompletionSource<T>();
		Far.Api.PostJob(() =>
		{
			try
			{
				task.SetResult(job());
			}
			catch (Exception exn)
			{
				task.SetException(exn);
			}
		});
		return task.Task;
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
		var task = new TaskCompletionSource<object>();
		Far.Api.PostJob(() =>
		{
			try
			{
				job();
				task.SetResult(null!);
			}
			catch (Exception exn)
			{
				task.SetException(exn);
			}
		});
		return task.Task;
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
		var task = new TaskCompletionSource<object>();
		Far.Api.PostJob(() =>
		{
			try
			{
				//! try because the job may fail before UI
				Far.Api.PostJob(() => task.TrySetResult(null!));
				job();
			}
			catch (Exception exn)
			{
				//! try because the task may complete on UI and the job may fail after
				if (!task.TrySetException(exn))
					throw;
			}
		});
		return task.Task;
	}

	/// <summary>
	/// Awaits the task and processes exceptions in the main thread.
	/// </summary>
	/// <param name="task">The task function.</param>
	/// <param name="error">The optional exception handler.</param>
	public static async void ExecuteAndCatch(Func<Task> task, Action<Exception>? error = null)
	{
		try
		{
			await task();
		}
		catch (Exception exn)
		{
			await Job(() =>
			{
				if (error is null)
					Far.Api.ShowError(null, exn);
				else
					error(exn);
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
		var task = new TaskCompletionSource<object>();
		Far.Api.PostJob(() =>
		{
			try
			{
				var wait = Works.Far2.Api.PostMacroWait(text);
				ThreadPool.QueueUserWorkItem(_ =>
				{
					wait.WaitOne();
					wait.Dispose();
					task.SetResult(null!);
				});
			}
			catch (Exception exn)
			{
				task.SetException(exn);
				return;
			}
		});
		return task.Task;
	}

	/// <summary>
	/// Starts a task which opens the editor if not yet and completes when it closes.
	/// </summary>
	/// <param name="editor">The editor to open.</param>
	/// <returns>The task which completes when the editor closes.</returns>
	public static Task Editor(IEditor editor)
	{
		var task = new TaskCompletionSource<object>();

		void onClosed(object? sender, EventArgs e)
		{
			editor.Closed -= onClosed;
			task.SetResult(null!);
		}

		Far.Api.PostJob(() =>
		{
			try
			{
				editor.Closed += onClosed;
				if (!editor.IsOpened)
					editor.Open();
			}
			catch (Exception exn)
			{
				editor.Closed -= onClosed;
				task.SetException(exn);
			}
		});

		return task.Task;
	}

	/// <summary>
	/// Starts a task which opens the viewer if not yet and completes when it closes.
	/// </summary>
	/// <param name="viewer">The viewer to open.</param>
	/// <returns>The task which completes when the viewer closes.</returns>
	public static Task Viewer(IViewer viewer)
	{
		var task = new TaskCompletionSource<object>();

		void onClosed(object? sender, EventArgs e)
		{
			viewer.Closed -= onClosed;
			task.SetResult(null!);
		}

		Far.Api.PostJob(() =>
		{
			try
			{
				viewer.Closed += onClosed;
				if (!viewer.IsOpened)
					viewer.Open();
			}
			catch (Exception exn)
			{
				viewer.Closed -= onClosed;
				task.SetException(exn);
			}
		});

		return task.Task;
	}

	/// <summary>
	/// Starts a task which opens the dialog and completes when it closes.
	/// </summary>
	/// <param name="dialog">The dialog to open.</param>
	/// <returns>The task which completes when the dialog closes.</returns>
	public static Task Dialog(IDialog dialog)
	{
		var task = new TaskCompletionSource<object>();

		void onClosed(object? sender, AnyEventArgs e)
		{
			dialog.Closed -= onClosed;
			task.SetResult(null!);
		}

		Far.Api.PostJob(() =>
		{
			try
			{
				dialog.Closed += onClosed;
				dialog.Open();
			}
			catch (Exception exn)
			{
				dialog.Closed -= onClosed;
				task.SetException(exn);
			}
		});

		return task.Task;
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
		var task = new TaskCompletionSource<T>();

		void onClosing(object? sender, ClosingEventArgs e)
		{
			try
			{
				var res = closing(e);
				if (e.Ignore)
					return;

				dialog.Closing -= onClosing;
				task.SetResult(res);
			}
			catch (Exception exn)
			{
				dialog.Closing -= onClosing;
				task.SetException(exn);
			}
		}

		Far.Api.PostJob(() =>
		{
			try
			{
				dialog.Closing += onClosing;
				dialog.Open();
			}
			catch (Exception exn)
			{
				dialog.Closing -= onClosing;
				task.SetException(exn);
			}
		});

		return task.Task;
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
		var task = new TaskCompletionSource<object>();
		//! post to avoid race for IsOpened
		Far.Api.PostJob(() =>
		{
			if (!panel.IsOpened)
			{
				task.SetResult(null!);
				return;
			}

			void onClosed(object? sender, EventArgs e)
			{
				panel.Closed -= onClosed;
				task.SetResult(null!);
			}

			panel.Closed += onClosed;
		});
		return task.Task;
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
		var task = new TaskCompletionSource<T>();
		//! post to avoid race for IsOpened
		Far.Api.PostJob(() =>
		{
			var result = default(T);

			if (!panel.IsOpened)
			{
				task.SetResult(result!);
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
				catch (Exception exn)
				{
					task.SetException(exn);
				}
			}

			void onClosed(object? sender, EventArgs e)
			{
				panel.Closing -= onClosing;
				panel.Closed -= onClosed;
				task.TrySetResult(result!);
			}

			panel.Closing += onClosing;
			panel.Closed += onClosed;
		});
		return task.Task;
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
