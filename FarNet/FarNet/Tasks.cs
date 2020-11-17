
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet.Forms;
using System;
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
		/// <returns>The task which completes when the function is called.</returns>
		/// <remarks>
		/// The job is posted by <see cref="IFar.PostJob(Action)"/>.
		/// It invokes the specified function and provides its result.
		/// The function may call Far as usual but it cannot open panels.
		/// </remarks>
		public static Task<T> Job<T>(Func<T> job)
		{
			var tcs = new TaskCompletionSource<T>();
			Far.Api.PostJob(() => {
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
		/// <returns>The task which completes when the action is called.</returns>
		/// <remarks>
		/// The job is posted by <see cref="IFar.PostJob(Action)"/>.
		/// It invokes the specified action.
		/// The action may call Far as usual but it cannot open panels.
		/// </remarks>
		public static Task Job(Action job)
		{
			var tcs = new TaskCompletionSource<int>();
			Far.Api.PostJob(() => {
				try
				{
					job();
					tcs.SetResult(0);
				}
				catch (Exception exn)
				{
					tcs.SetException(exn);
				}
			});
			return tcs.Task;
		}
		/// <summary>
		/// Creates a task which posts the specified macro.
		/// </summary>
		/// <param name="text">Macro text.</param>
		/// <returns>The task which completes when the macro is called.</returns>
		public static Task Macro(string text)
		{
			var tcs = new TaskCompletionSource<int>();
			Environment.SetEnvironmentVariable(_envMacroFlag, "0");
			try
			{
				Far.Api.PostMacro(text);
				Far.Api.PostMacro(_macroSetFlag);
				Task.Run(() =>
				{
					while (Environment.GetEnvironmentVariable(_envMacroFlag) != "1")
						Thread.Sleep(50);
					tcs.SetResult(0);
				});
			}
			catch (Exception exn)
			{
				tcs.SetException(exn);
			}
			return tcs.Task;
		}
		/// <summary>
		/// Creates a task which posts the specified keys.
		/// </summary>
		/// <param name="keys">Keys text.</param>
		/// <returns>The task which completes when the keys macro is called.</returns>
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
			var tcs = new TaskCompletionSource<int>();

			void onClosed(object sender, EventArgs e)
			{
				editor.Closed -= onClosed;
				tcs.SetResult(0);
			}

			Far.Api.PostJob(() => {
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

			Far.Api.PostJob(() => {
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
	}
}
