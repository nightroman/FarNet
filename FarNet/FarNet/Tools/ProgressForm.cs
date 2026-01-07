using FarNet.Forms;

namespace FarNet.Tools;

/// <summary>
/// A form to show progress of potentially long background jobs.
/// </summary>
/// <remarks>
/// <para>
/// This form should be created and shown once in the main thread.
/// Some members are designed for other threads:
/// normal cases: <see cref="Activity"/>, <see cref="SetProgressValue"/>, <see cref="Complete"/>;
/// cancel cases: <see cref="Close"/>, <see cref="IsClosed"/>, <see cref="CancellationToken"/>.
/// </para>
/// <para>
/// Typical 4-step scenario:
/// <ul>
/// <li>Create a progress form, do not show yet.</li>
/// <li>Start a job in another thread using this form for progress.</li>
/// <li>Sleep the main thread a little to let the fast job complete.</li>
/// <li>Show the form. The form is shown if the job is not complete.</li>
/// </ul>
/// </para>
/// </remarks>
public sealed class ProgressForm : Form, IProgress
{
	const int DefaultTimerInterval = 200;

	readonly Lock _lock = new();
	int _LineCount = 1;
	bool _isCompleted;
	bool _isCanceled;
	bool _isClosed;

	readonly Progress _progress = new();
	readonly Thread _mainThread = Thread.CurrentThread;
	readonly CancellationTokenSource _tokenSource = new();

	IText[]? _textActivity;
	IText? _textProgress;

	/// <summary>
	/// New progress form.
	/// </summary>
	/// <remarks>
	/// It should be created and shown in the main thread.
	/// </remarks>
	public ProgressForm()
	{
		CancellationToken = _tokenSource.Token;
	}

	/// <summary>
	/// Gets the cancellation token.
	/// </summary>
	/// <remarks>
	/// Instead of checking <see cref="IsClosed"/> jobs may check this token and exit if canceled.
	/// The token may be also used for registering actions called when canceled.
	/// </remarks>
	public CancellationToken CancellationToken { get; }

	/// <summary>
	/// Gets or sets the text line count.
	/// </summary>
	/// <remarks>
	/// It is set before the show.
	/// The default is 1.
	/// </remarks>
	public int LineCount
	{
		get => _LineCount;
		set
		{
			if (value < 1 || value > Progress.TEXT_HEIGHT)
				throw new ArgumentOutOfRangeException(nameof(value));

			_LineCount = value;
		}
	}

	/// <summary>
	/// Tells to show the <b>Cancel</b> button.
	/// </summary>
	/// <remarks>
	/// <para>
	/// True: users can cancel the form and job.
	/// The job should check <see cref="IsClosed"/> or <see cref="CancellationToken"/> and stop as soon as needed.
	/// </para>
	/// <para>
	/// False: users cannot cancel the form and job.
	/// The form is opened until <see cref="Complete"/> or <see cref="Close"/> is called.
	/// </para>
	/// </remarks>
	public bool CanCancel { get; set; }

	/// <summary>
	/// Called when the form is about to be canceled.
	/// </summary>
	/// <remarks>
	/// It may be used in order to confirm canceling.
	/// Set <see cref="ClosingEventArgs.Ignore"/> to stop canceling.
	/// </remarks>
	public event EventHandler<ClosingEventArgs>? Canceling;

	/// <summary>
	/// Gets true if a closing method has been called or a user has canceled the form.
	/// </summary>
	/// <remarks>
	/// Jobs should check this property periodically and exit as soon as it is true.
	/// Alternatively, jobs may use more common <see cref="CancellationToken"/>.
	/// </remarks>
	public bool IsClosed => _isClosed || _isCanceled;

	/// <summary>
	/// Gets true if the form and job completed normally.
	/// </summary>
	/// <remarks>
	/// If it is true then <see cref="IsClosed"/> is also true.
	/// </remarks>
	public bool IsCompleted => _isCompleted;

	/// <summary>
	/// Closes the form and cancels the token.
	/// </summary>
	/// <remarks>
	/// This method may be called by jobs in order to cancel the form.
	/// But normally they call <see cref="Complete"/> when they are done.
	/// <para>
	/// <see cref="Show()"/> and <see cref="Show(Task)"/> return false if the form is closed by this method.
	/// </para>
	/// </remarks>
	public override void Close()
	{
		lock (_lock)
		{
			if (_isClosed)
				return;

			_isClosed = true;
			_tokenSource.Cancel();

			//! mind another thread
			if (Thread.CurrentThread == _mainThread)
			{
				base.Close();
			}
			else
			{
				Far.Api.PostJob(base.Close);
			}
		}
	}

	/// <summary>
	/// Closes the form when the job is complete.
	/// </summary>
	/// <remarks>
	/// This method is called by jobs when they are done.
	/// With <see cref="Show()"/> this method must be called.
	/// With <see cref="Show(Task)"/> this method is optional.
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
	/// <returns>True if the <see cref="Complete"/> has been called and false in other cases.</returns>
	/// <remarks>
	/// This method should be called in the main thread after starting a job in another thread.
	/// Normally it shows the modal dialog and blocks the main thread.
	/// <para>
	/// The form is closed when a job calls the <see cref="Complete"/> or <see cref="Close"/> or
	/// a user cancels the form when <see cref="CanCancel"/> is true.
	/// </para>
	/// </remarks>
	public override bool Show()
	{
		if (_isClosed)
			return _isCompleted && !_isCanceled;

		Init();

		try
		{
			// show modal
			base.Show();
			return _isCompleted && !_isCanceled;
		}
		finally
		{
			// reset progress
			Far.Api.UI.SetProgressState(TaskbarProgressBarState.NoProgress);
			Far.Api.UI.SetProgressFlash();
		}
	}

	/// <summary>
	/// Shows the progress form or returns the result if the job is already done.
	/// </summary>
	/// <returns>True if the job completes and false in other cases.</returns>
	/// <param name="job">The potentially long job.</param>
	/// <remarks>
	/// <para>
	/// With this method the job does not have to call <see cref="Complete"/>.
	/// </para>
	/// <para>
	/// The job may throw <c>OperationCanceledException</c> in order to cancel the form.
	/// Other exceptions are treated as errors and thrown.
	/// Calling this method may need exception handling.
	/// </para>
	/// </remarks>
	public bool Show(Task job)
	{
		// start watching the task
		Exception? error = null;
		_ = Task.Run(async () =>
		{
			try
			{
				await job;
				Complete();
			}
			catch (Exception ex)
			{
				error = ex;
				Close();
			}
		},
		CancellationToken);

		// then show the form
		bool res = Show();
		if (error == null)
			return res;

		// treat some exceptions as cancel, mind wrappers, e.g. RuntimeException in PowerShell
		if (error is OperationCanceledException || error.InnerException is OperationCanceledException)
			return false;

		// throw other exceptions
		throw error;
	}

	void OnInitialized(object? sender, InitializedEventArgs e)
	{
		// do not show the form if it is already closed
		if (_isClosed)
			e.Ignore = true;
	}

	bool AbortCanceling()
	{
		if (Canceling == null)
			return false;

		var args = new ClosingEventArgs(null);
		Canceling(this, args);
		return args.Ignore;
	}

	void OnClosing(object? sender, ClosingEventArgs e)
	{
		lock (_lock)
		{
			// allow the dialog to close if the form is closed
			if (_isClosed)
				return;

			// do not close if cannot or aborted
			if (!CanCancel || AbortCanceling())
			{
				e.Ignore = true;
				return;
			}

			// flag
			_isCanceled = true;
			_tokenSource.Cancel();
		}
	}

	void OnTimer(object? sender, EventArgs e)
	{
		// if the form is closed and the dialog is still alive then close the dialog directly
		if (_isClosed)
		{
			Dialog.Close();
			return;
		}

		// show
		var lines = _progress.Build(out string progress, _textActivity!.Length);
		for (int iLine = 0; iLine < _LineCount && iLine < lines.Length; ++iLine)
			_textActivity[iLine].Text = lines[iLine];
		_textProgress!.Text = progress;
	}

	void Init()
	{
		Dialog.TimerInterval = DefaultTimerInterval;
		Dialog.KeepWindowTitle = true;

		SetSize(Progress.FORM_WIDTH, (CanCancel ? 7 : 5) + _LineCount);

		_textActivity = new IText[_LineCount];
		for (int iLine = 0; iLine < _LineCount; ++iLine)
			_textActivity[iLine] = Dialog.AddText(5, -1, 5 + Progress.TEXT_WIDTH - 1, string.Empty);
		_textProgress = Dialog.AddText(5, -1, 5 + Progress.TEXT_WIDTH - 1, string.Empty);

		if (CanCancel)
		{
			Dialog.AddText(5, -1, 0, string.Empty).Separator = 1;

			IButton button = Dialog.AddButton(0, -1, "Cancel");
			button.CenterGroup = true;
			Dialog.Default = button;
		}

		Dialog.Initialized += OnInitialized;
		Dialog.Closing += OnClosing;
		Dialog.Timer += OnTimer;
	}

	#region IProgress

	/// <summary>
	/// Gets or sets the current activity description.
	/// </summary>
	/// <remarks>
	/// This property is used by jobs.
	/// It is fine to change it frequently.
	/// </remarks>
	public string Activity
	{
		get { return _progress.Activity; }
		set { _progress.Activity = value; }
	}

	/// <summary>
	/// Sets the current progress information.
	/// </summary>
	/// <param name="currentValue">Progress current value, from 0 to the maximum.</param>
	/// <param name="maximumValue">Progress maximum value, positive or 0.</param>
	/// <remarks>
	/// This method is used by jobs.
	/// It is fine to call it frequently.
	/// </remarks>
	public void SetProgressValue(double currentValue, double maximumValue)
	{
		_progress.SetProgressValue(currentValue, maximumValue);
	}

	/// <summary>
	/// It is not used directly.
	/// </summary>
	public void ShowProgress()
	{ }

	#endregion
}
