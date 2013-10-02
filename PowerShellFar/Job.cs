
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2013 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Threading;
using FarNet;

/*
 * Test _090831_162451:
 * Start many jobs, e.g. run Test-Job-.ps1 a few times. Then exit Far and choose 'Ignore' when asked.
 * => Mind hanging, denied access files, not removed tmp*.tmp files, null reference, and etc.
 *
 * Issue _090903_115803:
 * Removed jobs startup code. For jobs with no UI it makes problems: if I set $ErrorActionPreference = 'Stop'
 * in startup code and then set $ErrorActionPreference = 'Continue' in jobs code then on not terminating
 * errors PS fails: "~ Cannot write errors, report to Microsoft support".
 *
 * _091006_191117
 * Synchronous Stop() tends to hang, so we use asynchronous way and wait for finish with sleeping loop.
 *
 * _091006_191214
 * It is possible that a job is auto-disposed and it throws from BeginStop\CoreStop\AssertNotDisposed.
 * This is presumably at StopJobsOnExit() only, so it is not a serious issue.
 */

namespace PowerShellFar
{
	/// <summary>PowerShellFar background job.</summary>
	/// <remarks>
	/// A job is created by <c>Start-FarJob</c> cmdlet (<see cref="Commands.StartFarJobCommand"/>)
	/// and automatically started immediately (normally) or started later by <see cref="StartJob"/> (advanced).
	/// Properties are mostly the same as properties of PowerShell job, but some of them have different types though names are the same.
	/// <para>
	/// There are two kind of Far jobs: UI jobs and output jobs.
	/// </para>
	/// <para>
	/// UI jobs are included in the background job list. Job output and errors are converted into
	/// formatted text redirected into a file. A file that can be opened in from a job list in a
	/// viewer during or after the job processing. UI jobs may be hidden (excluded from the list);
	/// they may get visible (included into the list) on errors.
	/// </para>
	/// <para>
	/// Output jobs are never included into the job list. A caller is completely responsible for
	/// their life cycle, for example he has to call <see cref="Dispose"/>. Processing of output
	/// is up to a user.
	/// </para>
	/// </remarks>
	/// <seealso cref="Commands.StartFarJobCommand"/>
	public sealed class Job : IDisposable
	{
		const string MenuFormatString = "{0,9} : {1,6} : {2}";
		const int NotifyPeriod = 3000;

		/// <summary>
		/// Gets available jobs.
		/// </summary>
		public static IList<Job> Jobs
		{
			get { return JobList.ToArray(); }
		}

		// UI job list controlled by a user
		static readonly List<Job> JobList = new List<Job>();

		// Last notification target
		static Job JobLastNotified;

		// Timer started for notifications
		static Timer Timer;

		// Job UI: the job is visible
		JobUI JobUI;

		// For UI job: keep succeeded job for this time
		int KeepSeconds;

		// Stopwatch counting job keeping time
		Stopwatch KeepStopwatch;

		// Engine stuff
		readonly Runspace Runspace;
		readonly PowerShell PowerShell;
		IAsyncResult InvokeResult;

		/// <summary>
		/// New job.
		/// </summary>
		/// <remarks>
		/// Keep seconds for UI-less jobs: 0 ~ hidden mode, in this case a job creates UI on errors, as it is not attended.
		/// Other UI-less jobs are completely owned creators.
		/// </remarks>
		internal Job(JobCommand command, object parameters, string name, bool ui, int keepSeconds)
		{
			JobCommand = command;
			Parameters = parameters;
			Name = name;
			KeepSeconds = keepSeconds;

			// create/open runspace
			//! *) Do not catch, if we fail, we fail and there is nothing to repair yet (not open)
			//! *) Use existing configuration, it is faster! Most of *-Far* cmdlets should not be used,
			//! but some of them can be used, e.g. Update-FarDescription; also we want to use ETS types,
			//! e.g. FarDescription property.
			if (ui)
			{
				JobUI = new JobUI();
				Runspace = RunspaceFactory.CreateRunspace(new FarHost(JobUI), Runspace.DefaultRunspace.RunspaceConfiguration);
			}
			else
			{
				//! DefaultHost is created internally. Perhaps it is reasonable to live with it, not with a custom host.
				Runspace = RunspaceFactory.CreateRunspace(Runspace.DefaultRunspace.RunspaceConfiguration);
			}
			Runspace.Open();

			// new shell with the command
			PowerShell = PowerShell.Create();
			PowerShell.Runspace = Runspace;
			JobCommand.Add(PowerShell);

			// add command parameters
			if (parameters != null)
			{
				IDictionary namedParameters = parameters as IDictionary;
				IList argumentList;
				if (namedParameters != null)
					PowerShell.AddParameters(namedParameters);
				else if ((argumentList = parameters as IList) != null)
					PowerShell.AddParameters(argumentList);
				else
					PowerShell.AddParameters(new object[] { parameters });
			}

			// UI: Write all output, including errors.
			if (JobUI != null)
			{
				PowerShell.Commands.AddCommand(A.OutCommand);
			}
			// Hidden: Write output to "Out-Null" to avoid memory use.
			else if (keepSeconds <= 0)
			{
				//! User can use his Out-Null
				PowerShell.AddCommand("Out-Null");
			}
			// Output: create it once: it is cumulative
			else
			{
				Output = new PSDataCollection<PSObject>();
			}
		}

		#region Public properties

		/// <summary>
		/// Gets the command that is invoked by this job.
		/// </summary>
		public string Command
		{
			get { return JobCommand.Command; }
		}

		/// <summary>
		/// Gets the friendly name to identify the job.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the wait handle that is signaled when job is finished.
		/// </summary>
		public WaitHandle Finished
		{
			get { return InvokeResult == null ? null : InvokeResult.AsyncWaitHandle; }
		}

		//! Used by Search-Regex-.ps1
		/// <summary>
		/// Gets the job command parameters.
		/// </summary>
		/// <remarks>
		/// <c>IDictionary</c> for named parameters, <c>IList</c> for arguments, or a single argument.
		/// <para>
		/// Note that parameters can be used also for output via class instance properties,
		/// normally when primary <see cref="Output"/> data should not be mixed with others.
		/// Mind thread safety issues when a job works with not thread safe instances.
		/// </para>
		/// </remarks>
		public object Parameters { get; private set; }

		//! Used by Search-Regex-.ps1
		/// <summary>
		/// Gets the status of the job.
		/// </summary>
		/// <remarks>
		/// Properties:
		/// <para>
		/// <c>State</c>: Gets the current job state: <c>NotStarted</c>, <c>Running</c>, <c>Stopping</c>, <c>Stopped</c>, <c>Failed</c>, and <c>Completed</c>.
		/// </para>
		/// <para>
		/// <c>Reason</c>: Gets the reason for the last state change if the state changed because of an error.
		/// </para>
		/// </remarks>
		public PSInvocationStateInfo JobStateInfo
		{
			get { return PowerShell.InvocationStateInfo; }
		}

		/// <summary>
		/// Output of the job started for output. It is null for other jobs.
		/// </summary>
		public PSDataCollection<PSObject> Output { get; private set; }

		/// <summary>
		/// Gets the buffer where debug information is stored.
		/// </summary>
		public PSDataCollection<DebugRecord> Debug
		{
			get { return PowerShell.Streams.Debug; }
		}

		/// <summary>
		/// Gets the buffer where error information is stored.
		/// </summary>
		public PSDataCollection<ErrorRecord> Error
		{
			get { return PowerShell.Streams.Error; }
		}

		/// <summary>
		/// Gets the buffer where progress information is stored.
		/// </summary>
		public PSDataCollection<ProgressRecord> Progress
		{
			get { return PowerShell.Streams.Progress; }
		}

		/// <summary>
		/// Gets the buffer where verbose information is stored.
		/// </summary>
		public PSDataCollection<VerboseRecord> Verbose
		{
			get { return PowerShell.Streams.Verbose; }
		}

		/// <summary>
		/// Gets the buffer where warning information is stored.
		/// </summary>
		public PSDataCollection<WarningRecord> Warning
		{
			get { return PowerShell.Streams.Warning; }
		}

		#endregion

		/// <summary>
		/// Job command.
		/// </summary>
		JobCommand JobCommand { get; set; }

		/// <summary>
		/// Job state text. Values: NotStarted, Running, Stopping, Stopped, Failed, Completed, and Errors (Completed with errors).
		/// </summary>
		string StateText
		{
			get
			{
				if (IsError && JobStateInfo.State == PSInvocationState.Completed)
					return "Errors";
				else
					return JobStateInfo.State.ToString();
			}
		}

		/// <summary>
		/// Output data stream length.
		/// </summary>
		long Length
		{
			get { return JobUI == null ? 0 : JobUI.Length; }
		}

		/// <summary>
		/// Is the job 'hidden' ~ no UI, no output?
		/// </summary>
		bool IsHidden
		{
			get { return JobUI == null && KeepSeconds <= 0; }
		}

		/// <summary>
		/// Is there any error?
		/// </summary>
		bool IsError
		{
			get { return JobUI != null && JobUI.HasError; }
		}

		/// <summary>
		/// Is the job running?
		/// </summary>
		bool IsRunning
		{
			get { return PowerShell.InvocationStateInfo.State == PSInvocationState.Running; }
		}

		/// <summary>
		/// Is the job finished?
		/// </summary>
		bool IsFinished
		{
			get
			{
				return
					PowerShell.InvocationStateInfo.State == PSInvocationState.Completed ||
					PowerShell.InvocationStateInfo.State == PSInvocationState.Failed ||
					PowerShell.InvocationStateInfo.State == PSInvocationState.Stopped;
			}
		}

		/// <summary>
		/// Is the job succeeded, i.e. completed with no errors?
		/// </summary>
		bool IsSucceeded
		{
			get { return JobStateInfo.State == PSInvocationState.Completed && !IsError; }
		}

		/// <summary>
		/// Output file name or null if output is not started.
		/// </summary>
		string FileName
		{
			get { return JobUI == null ? null : JobUI.FileName; }
		}

		/// <summary>
		/// Is <see cref="Job.Dispose"/> called?
		/// </summary>
		bool Disposed;

		/// <summary>
		/// Gets a text line.
		/// </summary>
		string ToLine(int maxLength)
		{
			string r = string.IsNullOrEmpty(Name) ? JobCommand.Command.Trim() : Name;

			if (r.Length > maxLength)
				r = r.Substring(0, maxLength - 3) + "...";

			return Regex.Replace(r, @"\s+", " ");
		}

		/// <summary>
		/// Disposes the job and removes it from the job list.
		/// </summary>
		/// <remarks>
		/// If you own this object, e.g. call <c>Start-FarJob</c> with <see cref="Commands.StartFarJobCommand.Output"/> switch
		/// then you should dispose it after use. It is OK to call this for already disposed object.
		/// </remarks>
		public void Dispose()
		{
			if (Disposed)
				return;

			Disposed = true;

			if (PowerShell != null)
				PowerShell.Dispose();

			if (Runspace != null)
				Runspace.Close();

			if (JobUI != null)
			{
				JobUI.Close();

				if (JobUI.FileName != null)
					File.Delete(JobUI.FileName);
			}
			
			JobList.Remove(this);
		}

		/// <summary>
		/// Stops the job.
		/// </summary>
		public void StopJob()
		{
			// _091006_191117, _091006_191214
			try
			{
				PowerShell.BeginStop(PowerShell.EndStop, null);
				while (!IsFinished)
					Thread.Sleep(50);
			}
			catch (PSObjectDisposedException)
			{ }

			if (JobUI != null)
				JobUI.HasError = true;
		}

		internal static void ShowJobs()
		{
			IMenu menu = Far.Api.CreateMenu();
			menu.Title = Res.BackgroundJobs;
			menu.ShowAmpersands = true;
			menu.HelpTopic = Far.Api.GetHelpTopic("MenuBackgroundJobs");
			menu.AddKey(KeyCode.F3);
			menu.AddKey(KeyCode.F5);
			menu.AddKey(KeyCode.Delete);
			menu.AddKey(KeyCode.Delete, ControlKeyStates.ShiftPressed);

			for (int show = 0; ; ++show)
			{
				WatchJobs();
				if (show > 0 && JobList.Count == 0)
					return;

				menu.Items.Clear();
				FarItem item = menu.Add(string.Format(null, MenuFormatString, "State", "Output", "Name/Command"));
				item.Disabled = true;
				foreach (Job job in JobList)
				{
					item = menu.Add(string.Format(null, MenuFormatString, job.StateText, job.Length, job.ToLine(100)));
					item.Data = job;
				}

				while (menu.Show())
				{
					// refresh
					if (menu.Key.Is(KeyCode.F5))
					{
						menu.Items.Clear();
						break;
					}

					Job job = (Job)menu.SelectedData;
					if (job == null)
						break;

					// delete
					if (menu.Key.Is(KeyCode.Delete))
					{
						if (job.IsRunning)
						{
							job.StopJob();
							if (job.Length > 0)
								break;
						}
						job.Dispose();
						break;
					}

					// delete all
					if (menu.Key.IsShift(KeyCode.Delete))
					{
						// copy and then traverse
						var jobsToKill = new List<Job>(JobList);
						foreach (Job jobToKill in jobsToKill)
						{
							if (jobToKill.IsRunning)
							{
								jobToKill.StopJob();
								if (jobToKill.Length > 0)
									continue;
							}
							jobToKill.Dispose();
						}
						break;
					}

					// view
					if (job.FileName != null)
					{
						// file can be removed if the job is discarded
						if (!File.Exists(job.FileName))
							break;

						// file exists, view it
						IViewer v = Far.Api.CreateViewer();
						v.FileName = job.FileName;
						v.DisableHistory = true;
						if (menu.Key == new KeyData(KeyCode.F3))
						{
							v.Open(OpenMode.Modal);
							break;
						}
						else
						{
							v.Open(OpenMode.None);
							return;
						}
					}
				}
				if (menu.Selected < 0)
					return;
			}
		}

		/// <summary>
		/// Called by the timer thread, must only post the call for Far
		/// </summary>
		static void AsyncTimerCallback(object state)
		{
			Far.Api.PostJob(WatchJobs);
		}

		/// <summary>
		/// Watch the jobs, notifies about finished, removes discardable and disposed.
		/// </summary>
		/// <remarks>
		/// Normally it is posted by <see cref="AsyncTimerCallback"/>.
		/// It is ok to call it directly but from the main thread only.
		/// </remarks>
		static void WatchJobs()
		{
			// watch the jobs
			var finished = new List<Job>(JobList.Count);
			for (int iJob = 0; iJob < JobList.Count; ++iJob)
			{
				Job job = JobList[iJob];

				// process alive
				if (!job.Disposed)
				{
					if (job.IsSucceeded && (job.KeepSeconds <= 0 || job.KeepStopwatch != null && job.KeepStopwatch.Elapsed.TotalSeconds > job.KeepSeconds))
					{
						// kill succeeded
						job.Dispose();
					}
					else if (job.IsFinished)
					{
						// collect finished
						finished.Add(job);
					}
				}

				// remove killed
				if (job.Disposed)
				{
					JobList.RemoveAt(iJob);
					--iJob;
				}
			}

			// kill/install the timer
			if (finished.Count == 0)
			{
				JobLastNotified = null;
				if (Timer != null)
				{
					Timer.Dispose();
					Timer = null;

					// win7 NoProgress
					Far.Api.UI.SetProgressState(TaskbarProgressBarState.NoProgress);
				}
			}
			else
			{
				// find 'next' finished to notify
				int index = finished.IndexOf(JobLastNotified);
				if (++index >= finished.Count)
					index = 0;

				// notified job, start its stopwatch now
				JobLastNotified = finished[index];
				if (JobLastNotified.KeepSeconds > 0 && JobLastNotified.KeepStopwatch == null)
					JobLastNotified.KeepStopwatch = Stopwatch.StartNew();

				// notify
				Far.Api.UI.WindowTitle = JobLastNotified.StateText + ": " + JobLastNotified.ToLine(100);

				// win7
				Far.Api.UI.SetProgressValue(1, 1);
				Far.Api.UI.SetProgressState(JobLastNotified.IsSucceeded ? TaskbarProgressBarState.Normal : TaskbarProgressBarState.Error);

				// install the timer
				if (Timer == null)
					Timer = new Timer(AsyncTimerCallback, null, NotifyPeriod, NotifyPeriod);
			}
		}

		/// <summary>
		/// Starts the job.
		/// </summary>
		public void StartJob()
		{
			// register UI job
			if (JobUI != null)
				JobList.Add(this);

			// invoke async
			if (Output == null)
				InvokeResult = PowerShell.BeginInvoke<PSObject>(null, null, AsyncInvoke, null);
			else
				InvokeResult = PowerShell.BeginInvoke<PSObject, PSObject>(null, Output, null, AsyncInvoke, null);
		}

		//! Called by the job thread.
		void AsyncInvoke(IAsyncResult ar)
		{
			// end; it may throw, e.g. if stopped
			try
			{
				PowerShell.EndInvoke(ar);
			}
			catch (RuntimeException)
			{ }

			// state
			switch (PowerShell.InvocationStateInfo.State)
			{
				case PSInvocationState.Completed:
					if (IsHidden)
					{
						// OK: discard
						if (PowerShell.Streams.Error.Count == 0)
						{
							Dispose();
							return;
						}

						// KO: make it UI
						JobUI = new JobUI();
						JobList.Add(this);
						JobUI.HasError = true;
						A.WriteErrors(JobUI.GetWriter(), PowerShell.Streams.Error);
					}
					break;
				case PSInvocationState.Failed:

					// make UI for a hidden job, and (!) write not terminating errors first
					if (IsHidden)
					{
						JobUI = new JobUI();
						JobList.Add(this);
						A.WriteErrors(JobUI.GetWriter(), PowerShell.Streams.Error);
					}

					// UI
					if (JobUI != null)
					{
						JobUI.HasError = true;
						A.WriteException(JobUI.GetWriter(), PowerShell.InvocationStateInfo.Reason);
					}
					break;
			}

			// UI
			if (JobUI != null)
			{
				// close not needed now UI
				JobUI.Close();

				// post notificator
				Far.Api.PostJob(WatchJobs);
			}
		}

		[EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
		internal static void StopJobsOnExit()
		{
			while (JobList.Count > 0)
			{
				Job job = JobList[0];

				if (!job.IsRunning && job.Length == 0)
				{
					job.Dispose();
					JobList.RemoveAt(0);
					continue;
				}

				string message = string.Format(null, @"
Job:
{0}

State: {1}
Output: {2}

Abort: discard the job
Retry: wait for exit or view output
Ignore: discard all jobs and output

", job.ToLine(100), job.StateText, job.Length);

				string title = "Background job";
				Far.Api.UI.WindowTitle = title;

				switch (Far.Api.Message(message, title, MessageOptions.Gui | MessageOptions.AbortRetryIgnore))
				{
					case 0:
						{
							if (job.IsRunning)
								job.StopJob();
							job.Dispose();
							JobList.RemoveAt(0);
						}
						break;
					case 1:
						if (job.IsRunning)
						{
							Far.Api.UI.WindowTitle = "Waiting for a background job...";
							job.Finished.WaitOne();
						}
						else
						{
							if (job.JobUI.Length > 0)
								My.ProcessEx.StartNotepad(job.FileName).WaitForExit();

							job.Dispose();
							JobList.RemoveAt(0);
						}
						break;
					default:
						{
							foreach (Job j in JobList)
							{
								j.StopJob(); //!
								j.Dispose();
							}
							JobList.Clear();
						}
						return;
				}
			}
		}

		internal static bool CanExit()
		{
			// show jobs
			if (JobList.Count > 0)
			{
				ShowJobs();
				if (Job.JobList.Count > 0)
					return false;
			}
			return true;
		}
	}
}
