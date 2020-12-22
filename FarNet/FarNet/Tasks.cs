
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet.Forms;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FarNet
{
	/// <summary>
	/// Task helpers.
	/// </summary>
	public static class Tasks
	{
		const string _envMacroFlag = "FarNet.Tasks.macro";
		const string _macroSetFlag = "mf.env('FarNet.Tasks.macro', 1, '1')";
		/// <summary>
		/// Creates a task with the specified function.
		/// </summary>
		/// <typeparam name="T">The function result.</typeparam>
		/// <param name="job">The function to be posted.</param>
		/// <returns>The task which completes when the function completes.</returns>
		public static Task<T> Job<T>(Func<T> job)
		{
			var tcs = new TaskCompletionSource<T>();
			Far.Api.PostJob(() =>
			{
				try
				{
					tcs.SetResult(job());
				}
				catch (Exception exn)
				{
					tcs.SetException(exn);
				}
			});
			return tcs.Task;
		}
		/// <summary>
		/// Creates a task with the specified action.
		/// </summary>
		/// <param name="job">The action to be posted.</param>
		/// <returns>The task which completes when the action completes.</returns>
		public static Task Job(Action job)
		{
			var tcs = new TaskCompletionSource<object>();
			Far.Api.PostJob(() =>
			{
				try
				{
					job();
					tcs.SetResult(null);
				}
				catch (Exception exn)
				{
					tcs.SetException(exn);
				}
			});
			return tcs.Task;
		}
		/// <summary>
		/// Creates a task which runs the specified action.
		/// </summary>
		/// <param name="job">The action to run.</param>
		/// <returns>The task which completes when the core gets control.</returns>
		/// <remarks>
		/// The task completes when the core gets control,
		/// either on shown modal UI or the job end,
		/// whatever happens first.
		/// <para>
		/// The job is supposed to show some modal UI and
		/// let next tasks work for automation, tests, etc.
		/// </para>
		/// </remarks>
		public static Task Run(Action job)
		{
			var tcs = new TaskCompletionSource<object>();
			Far.Api.PostJob(() =>
			{
				try
				{
					//! try because the job may fail before UI
					Far.Api.PostJob(() => tcs.TrySetResult(null));
					job();
				}
				catch (Exception exn)
				{
					//! try because the task may complete on UI and the job may fail after
					if (!tcs.TrySetException(exn))
						throw exn;
				}
			});
			return tcs.Task;
		}
		/// <summary>
		/// Creates a task which posts the specified macro.
		/// </summary>
		/// <param name="text">Macro text.</param>
		/// <returns>The task which completes when the macro completes.</returns>
		public static Task Macro(string text)
		{
			Environment.SetEnvironmentVariable(_envMacroFlag, "0");
			var task = new TaskCompletionSource<object>();
			Far.Api.PostJob(() => {
				try
				{
					Far.Api.PostMacro(text);
					Far.Api.PostMacro(_macroSetFlag);
				}
				catch (Exception exn)
				{
					task.SetException(exn);
					return;
				}
				ThreadPool.QueueUserWorkItem(_ =>
				{
					while (Environment.GetEnvironmentVariable(_envMacroFlag) != "1")
						Thread.Sleep(10);
					task.SetResult(null);
				});
			});
			return task.Task;
		}
		/// <summary>
		/// Creates a task which posts the specified keys.
		/// </summary>
		/// <param name="keys">Keys text.</param>
		/// <returns>The task which completes when the keys complete.</returns>
		public static Task Keys(string keys)
		{
			return Macro($"Keys[[{keys}]]");
		}
		/// <summary>
		/// Creates a task which opens the editor and completes when it closes.
		/// </summary>
		/// <param name="editor">The editor.</param>
		/// <returns>The task which completes when the editor closes.</returns>
		public static Task Editor(IEditor editor)
		{
			var tcs = new TaskCompletionSource<object>();

			void onClosed(object sender, EventArgs e)
			{
				editor.Closed -= onClosed;
				tcs.SetResult(null);
			}

			Far.Api.PostJob(() =>
			{
				try
				{
					editor.Closed += onClosed;
					editor.Open();
				}
				catch (Exception exn)
				{
					editor.Closed -= onClosed;
					tcs.SetException(exn);
				}
			});

			return tcs.Task;
		}
		/// <summary>
		/// Creates a task which opens the dialog with the specified closing function.
		/// </summary>
		/// <typeparam name="T">The closing result.</typeparam>
		/// <param name="dialog">The dialog to be opened.</param>
		/// <param name="closing">The closing function.</param>
		/// <returns>The task which completes when the dialog closes.</returns>
		/// <remarks>
		/// The closing function is similar to <see cref="IDialog.Closing"/> handlers but with a result.
		/// The result should take cancel into account (<see cref="AnyEventArgs.Control"/> is null).
		/// For example null on cancel and not null otherwise.
		/// </remarks>
		public static Task<T> Dialog<T>(IDialog dialog, Func<ClosingEventArgs, T> closing)
		{
			var tcs = new TaskCompletionSource<T>();

			void onClosing(object sender, ClosingEventArgs e)
			{
				try
				{
					var res = closing(e);
					if (e.Ignore)
						return;

					dialog.Closing -= onClosing;
					tcs.SetResult(res);
				}
				catch (Exception exn)
				{
					dialog.Closing -= onClosing;
					tcs.SetException(exn);
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
					tcs.SetException(exn);
				}
			});

			return tcs.Task;
		}
		/// <summary>
		/// Creates a task which opens the dialog and completes when it closes.
		/// </summary>
		/// <param name="dialog">The dialog to open.</param>
		/// <returns>The task which completes when the dialog closes.</returns>
		public static Task Dialog(IDialog dialog)
		{
			var tcs = new TaskCompletionSource<object>();

			void onClosed(object sender, AnyEventArgs e)
			{
				dialog.Closed -= onClosed;
				tcs.SetResult(null);
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
					tcs.SetException(exn);
				}
			});

			return tcs.Task;
		}
		/// <summary>
		/// Creates a task which waits for the panel closing.
		/// </summary>
		/// <param name="panel">The panel to await.</param>
		/// <returns>The task which completes when the panel closes.</returns>
		/// <remarks>
		/// The panel is opened automatically if it is not yet opened.
		/// </remarks>
		public static Task Panel(Panel panel)
		{
			var tcs = new TaskCompletionSource<object>();

			void onClosed(object sender, EventArgs e)
			{
				panel.Closed -= onClosed;
				tcs.SetResult(null);
			}

			Far.Api.PostStep(() =>
			{
				try
				{
					panel.Closed += onClosed;
					if (!panel.IsOpened)
						panel.Open();
				}
				catch (Exception exn)
				{
					panel.Closed -= onClosed;
					tcs.SetException(exn);
				}
			});

			return tcs.Task;
		}
		/// <summary>
		/// Waits for the predicate job returning true.
		/// </summary>
		/// <param name="delay">Milliseconds to sleep before the first check.</param>
		/// <param name="sleep">Milliseconds to sleep when the predicate returns false.</param>
		/// <param name="timeout">Maximum waiting time in milliseconds, non positive ~ infinite.</param>
		/// <param name="predicate">Returns true to stop waiting.</param>
		/// <returns>True if the predicate returns true before the time is out.</returns>
		public static async Task<bool> Wait(int delay, int sleep, int timeout, Func<bool> predicate)
		{
			if (delay > 0)
				await Task.Delay(delay);

			var span = timeout > 0 ? TimeSpan.FromMilliseconds(timeout) : TimeSpan.MaxValue;
			var time = Stopwatch.StartNew();
			for(; ; )
			{
				var ok = await Job(predicate);
				if (ok)
					return true;

				if (time.Elapsed > span)
					return false;

				await Task.Delay(sleep);
			}
		}
	}
}
