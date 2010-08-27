
/*
FarNet.Tools library for FarNet
Copyright (c) 2010 Roman Kuzmin
*/

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using FarNet.Forms;

namespace FarNet.Tools
{
	/// <summary>
	/// A form to show progress or activity of potentially long background jobs.
	/// </summary>
	/// <remarks>
	/// This form should be created and shown in the main thread.
	/// Some members are designed for use in other threads, for example:
	/// normal cases: <see cref="SetProgressValue"/>, <see cref="Activity"/>, <see cref="Complete"/>;
	/// cancellation cases: <see cref="Close"/>, <see cref="IsClosed"/>, <see cref="Cancelled"/>.
	/// <para>
	/// The form can be shown once and cannot be reused after closing.
	/// </para>
	/// <para>
	/// The standard scenario:
	/// <ul>
	/// <li>create a progress form but do not show yet;</li>
	/// <li>start a job in another thread and give it this form;</li>
	/// <li>let the main thread to sleep a bit: a fast job may complete;</li>
	/// <li>show the form; the progress form is shown if a job is not yet done.</li>
	/// </ul>
	/// </para>
	/// There is yet another simpler scenario using the <see cref="Invoke"/>, see remarks there.
	/// </remarks>
	public sealed class ProgressForm : Form
	{
		const char EMPTY_BLOCK = '\x2591';
		const char SOLID_BLOCK = '\x2588';

		const int FORM_WIDTH = 76;
		const int TEXT_WIDTH = FORM_WIDTH - 10;
		const int PERCENT_WIDTH = 4;
		const int PROGRESS_WIDTH = TEXT_WIDTH - PERCENT_WIDTH;

		object _lock = new object();
		bool _isCompleted;
		bool _isClosed;

		readonly Thread _mainThread;
		int _percentage = -1;
		Stopwatch _stopwatch;

		Thread _jobThread;
		Exception _jobError;

		IText _textActivity;
		IText _textProgress;
		IText _textPercent;

		/// <summary>
		/// New progress form.
		/// </summary>
		/// <remarks>
		/// It should be created and then shown in the main thread.
		/// </remarks>
		public ProgressForm()
		{
			_mainThread = Thread.CurrentThread;
		}

		/// <summary>
		/// Gets or sets the current activity description.
		/// </summary>
		/// <remarks>
		/// This property is designed for jobs in addition to the <see cref="SetProgressValue"/>.
		/// </remarks>
		public string Activity { get; set; }

		/// <summary>
		/// Tells to show the <b>Cancel</b> button.
		/// </summary>
		/// <remarks>
		/// False: a user cannot cancel the progress form and jobs in progress.
		/// The form is opened until <see cref="Complete"/> or <see cref="Close"/> is called.
		/// <para>
		/// True: a user can cancel the progress form.
		/// A job has to support this: it should check the <see cref="IsClosed"/> periodically
		/// or listen to the <see cref="Cancelled"/> event; if any of these happens the job
		/// has to exit as soon as possible.
		/// </para>
		/// </remarks>
		public bool CanCancel { get; set; }

		/// <summary>
		/// Called when the form is cancelled by a user or closed by the <see cref="Close"/>.
		/// </summary>
		public event EventHandler Cancelled;

		/// <summary>
		/// Gets true if a closing method has been called or a user has cancelled the form.
		/// </summary>
		/// <remarks>
		/// Jobs may check this property periodically and exit as soon as it is true.
		/// Alternatively, they may listen to the <see cref="Cancelled"/> event.
		/// </remarks>
		public bool IsClosed
		{
			get { return _isClosed; }
		}

		/// <summary>
		/// Gets true if the <see cref="Complete"/> has been called.
		/// </summary>
		/// <remarks>
		/// If it is true then <see cref="IsClosed"/> is also true.
		/// </remarks>
		public bool IsCompleted
		{
			get { return _isCompleted; }
		}

		/// <summary>
		/// Closes the form and triggers the <see cref="Cancelled"/> event.
		/// </summary>
		/// <remarks>
		/// This method is thread safe and can be called from jobs.
		/// But normally jobs should call <see cref="Complete"/> when they are done.
		/// <para>
		/// The <see cref="Show"/> returns false if the form is closed by this method.
		/// </para>
		/// </remarks>
		public override void Close()
		{
			lock (_lock)
			{
				if (_isClosed)
					return;

				_isClosed = true;

				if (_jobThread != null)
					_jobThread.Abort();

				//! mind another thread
				if (Thread.CurrentThread == _mainThread)
				{
					base.Close();
				}
				else
				{
					Far.Net.PostJob(delegate
					{
						base.Close();
					});
				}
			}
		}

		/// <summary>
		/// Closes the form when the job is complete.
		/// </summary>
		/// <remarks>
		/// This method is thread safe and designed for jobs.
		/// Normally when a job is done it calls this method.
		/// The <see cref="Show"/> returns true if the form is closed by this method.
		/// </remarks>
		public void Complete()
		{
			// lock it: Close() can be in progress right now; wait and do nothing then
			lock (_lock)
			{
				if (_isClosed)
					return;

				_isCompleted = true;
				Close();
			}
		}

		/// <summary>
		/// Shows the progress form or returns the result if the job is already done.
		/// </summary>
		/// <returns>True if the <see cref="Complete"/> has been called and false in all other cases.</returns>
		/// <remarks>
		/// This method should be called in the main thread after starting a job in another thread.
		/// Normally it shows the modal dialog and blocks the main thread.
		/// <para>
		/// The form is closed when a job calls the <see cref="Complete"/> or <see cref="Close"/> or
		/// a user cancels the form when <see cref="CanCancel"/> is true.
		/// </para>
		/// <para>
		/// If a job is fast and has already closed the form this methods returns immediately without showing a dialog.
		/// </para>
		/// </remarks>
		public override bool Show()
		{
			if (_isClosed)
				return _isCompleted;

			_stopwatch = Stopwatch.StartNew();

			Init();

			try
			{
				base.Show();
				return _isCompleted;
			}
			finally
			{
				Far.Net.UI.SetProgressState(TaskbarProgressBarState.NoProgress);
				Far.Net.UI.SetProgressFlash();
			}
		}

		/// <summary>
		/// Sets the progress values.
		/// </summary>
		/// <remarks>
		/// This method is thread safe and designed for jobs.
		/// <para>
		/// While this method is not called the form displays the elapsed time.
		/// Once this is called then the progress bar and percentage are shown.
		/// </para>
		/// <para>
		/// It is fine to call this frequently.
		/// The form is updated only periodically.
		/// </para>
		/// </remarks>
		public void SetProgressValue(int currentValue, int maximumValue)
		{
			if (currentValue <= 0)
			{
				_percentage = 0;
			}
			else if (maximumValue <= 0 || currentValue >= maximumValue)
			{
				_percentage = 100;
			}
			else
			{
				_percentage = currentValue * 100 / maximumValue;
			}
		}

		/// <summary>
		/// Sets the progress <c>long</c> values. See the <see cref="SetProgressValue"/>.
		/// </summary>
		public void SetProgressInt64(long currentValue, long maximumValue)
		{
			if (currentValue <= 0)
			{
				_percentage = 0;
			}
			else if (maximumValue <= 0 || currentValue >= maximumValue)
			{
				_percentage = 100;
			}
			else
			{
				_percentage = (int)(currentValue * 100 / maximumValue);
			}
		}

		void OnInitialized(object sender, InitializedEventArgs e)
		{
			// do not show the form if it is already closed
			if (_isClosed)
				e.Ignore = true;
		}

		void OnClose(object sender, EventArgs e)
		{
			if (Cancelled != null)
				Cancelled(this, null);

			Close();
		}

		void OnClosing(object sender, ClosingEventArgs e)
		{
			// allow the dialog to close if the form is closed
			if (_isClosed)
				return;

			// do not close if the form cannot cancel
			if (!CanCancel)
			{
				e.Ignore = true;
				return;
			}

			// abort
			if (_jobThread != null)
				_jobThread.Abort();

			// notify
			if (Cancelled != null)
				Cancelled(this, null);
		}

		void OnIdled(object sender, EventArgs e)
		{
			// if the form is closed and the dialog is still alive the closed the dialog directly
			if (_isClosed)
			{
				Dialog.Close();
				return;
			}

			// show activity
			{
				var activity = Activity ?? string.Empty;
				if (activity.Length > TEXT_WIDTH)
					activity = activity.Substring(0, TEXT_WIDTH - 3) + "...";
				_textActivity.Text = activity;
			}

			// show percentage or elapsed time
			if (_percentage >= 0)
			{
				// number of chars to fill
				int n = PROGRESS_WIDTH * _percentage / 100;

				// do not fill too much
				if (n > PROGRESS_WIDTH)
				{
					n = PROGRESS_WIDTH;
				}
				// leave 1 not filled
				else if (n == PROGRESS_WIDTH)
				{
					if (_percentage < 100)
						--n;
				}
				// fill at least 1
				else if (n == 0)
				{
					if (_percentage > 0)
						n = 1;
				}

				_textProgress.Text = new string(SOLID_BLOCK, n) + new string(EMPTY_BLOCK, PROGRESS_WIDTH - n);
				_textPercent.Text = string.Format(CultureInfo.InvariantCulture, "{0,3}%", _percentage);

				Far.Net.UI.SetProgressValue(_percentage, 100);
			}
			else
			{
				// use new TimeSpan with 'int' number of seconds, i.e. get 00:00:11, not 00:00:11.123456
				_textProgress.Text = (new TimeSpan(0, 0, (int)_stopwatch.Elapsed.TotalSeconds)).ToString();
				_textPercent.Text = string.Empty;
			}
		}

		void Init()
		{
			SetSize(FORM_WIDTH, (CanCancel ? 8 : 6));

			_textActivity = Dialog.AddText(5, -1, 5 + TEXT_WIDTH - 1, Activity ?? string.Empty);
			_textProgress = Dialog.AddText(5, -1, 5 + PROGRESS_WIDTH - 1, string.Empty);
			int x = _textProgress.Rect.Right + 1;
			_textPercent = Dialog.AddText(x, 0, x + (PERCENT_WIDTH - 1), string.Empty);

			if (CanCancel)
			{
				Dialog.AddText(5, -1, 0, string.Empty).Separator = 1;

				IButton button = Dialog.AddButton(0, -1, _jobThread == null ? "Cancel" : "Abort");
				button.CenterGroup = true;
				Dialog.Default = button;

				button.ButtonClicked += OnClose;
			}

			Dialog.Initialized += OnInitialized;
			Dialog.Closing += OnClosing;
			Dialog.Idled += OnIdled;
		}

		/// <summary>
		/// Invokes the job in a new thread (simplified scenario with optional job thread abortion by a user).
		/// </summary>
		/// <param name="job">The job action delegate to be invoked in a new thread. It should either complete or throw any exception.</param>
		/// <returns>Null if the job has completed or an exception thrown by the job or the <see cref="OperationCanceledException"/>.</returns>
		/// <remarks>
		/// This way is much simpler than the standard 4-steps scenario and it is recommended for not abortable jobs.
		/// <para>
		/// If the <see cref="CanCancel"/> is true then on user cancellation the job thread is aborted.
		/// It many cases this seems to be fine but the job has to be carefully designed for that.
		/// In particular the <see cref="ThreadAbortException"/> can be thrown at any moment.
		/// If there are potential unwanted effects of job abortion then do not use this way.
		/// </para>
		/// <para>
		/// This way is not suitable for PowerShell scripts in any case.
		/// Scripts should use the standard 4-steps scenario with standard PowerShell or simple PowerShellFar background jobs.
		/// </para>
		/// </remarks>
		public Exception Invoke(ThreadStart job)
		{
			// share the new thread and start it
			_jobThread = new Thread(() => Job(job));
			_jobThread.Start();

			// wait a little bit
			Thread.Sleep(500);
			
			// show the form and return null if it is completed
			if (Show())
				return null;

			// get the error
			return _jobError ?? new OperationCanceledException();
		}

		void Job(ThreadStart job)
		{
			try
			{
				// do the job in this thread
				job();
				
				// done, complete and return
				Complete();
				return;
			}
			catch (ThreadAbortException)
			{
				// convert to cancelled
				_jobError = new OperationCanceledException();
			}
			catch (Exception ex)
			{
				// to be returned by Invoke()
				_jobError = ex;
			}
			
			// close on errors
			Close();
		}

	}
}
