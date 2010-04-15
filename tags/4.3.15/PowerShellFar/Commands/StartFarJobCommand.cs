/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections;
using System.ComponentModel;
using System.Management.Automation;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// Start-FarJob command.
	/// Starts a Far background job.
	/// </summary>
	/// <remarks>
	/// It helps to create a background job with available parameters. Note:
	/// PowerShellFar background jobs are simple jobs that use PowerShell engine
	/// only and oriented for no output or formatted text output. In contrast,
	/// standard PowerShell background jobs require WSMan and output objects.
	/// </remarks>
	[Description("Starts a Far background job.")]
	public sealed class StartFarJobCommand : BaseCmdlet
	{
		///
		public StartFarJobCommand()
		{
			KeepSeconds = int.MaxValue;
		}

		/// <summary>
		/// See <see cref="JobCommand"/>.
		/// </summary>
		[Parameter(Position = 0, Mandatory = true, HelpMessage = "A command name or a script block.")]
		public JobCommand Command { get; set; }

		/// <summary>
		/// Command parameters. <c>IDictionary</c> for named parameters, <c>IList</c> for arguments, or a single argument.
		/// </summary>
		[Parameter(Position = 1, HelpMessage = "Command parameters. IDictionary for named parameters, IList for arguments, or a single argument.")]
		public PSObject Parameters { get; set; }

		/// <summary>
		/// Job friendly name to display.
		/// </summary>
		[Parameter(HelpMessage = "Job friendly name to display.")]
		public string Name { get; set; }

		/// <summary>
		/// Starts and returns the job with exposed <see cref="Job.Output"/>.
		/// </summary>
		/// <remarks>
		/// You have to <see cref="Job.Dispose"/> the job yourself after use.
		/// </remarks>
		[Parameter(HelpMessage = "Starts and returns the job with exposed Output. You Dispose() the job.")]
		public SwitchParameter Output { get; set; }

		/// <summary>
		/// Returns not yet started job for advanced setup.
		/// </summary>
		/// <remarks>
		/// It is similar to <see cref="Output"/> but the job is returned not started.
		/// You should <see cref="Job.StartJob"/> and <see cref="Job.Dispose"/> it yourself.
		/// </remarks>
		[Parameter(HelpMessage = "Returns not yet started job with exposed Output. You StartJob() and Dispose() the job.")]
		public SwitchParameter Return { get; set; }

		/// <summary>
		/// Started job is not returned, not shown in the list, output is discarded and succeeded job is disposed.
		/// </summary>
		/// <remarks>
		/// If the job fails or finishes with errors it is included in the list so that errors can be investigated.
		/// <para>
		/// For a hidden job parameters <see cref="Output"/>, <see cref="Return"/>, and <see cref="KeepSeconds"/> are ignored.
		/// </para>
		/// </remarks>
		[Parameter(HelpMessage = "Job is not included in the list, its output is discarded and 'Completed' job is disposed.")]
		public SwitchParameter Hidden { get; set; }

		/// <summary>
		/// Tells to keep succeeded job only for specified number of seconds.
		/// </summary>
		/// <remarks>
		/// Set 0 to remove the succeeded job immediately.
		/// <para>
		/// Jobs with errors are not removed automatically, you should remove them from the list.
		/// </para>
		/// <para>
		/// Stopwatch is started when the first job notification is shown in the console title.
		/// </para>
		/// </remarks>
		[Parameter(HelpMessage = "Tells to keep succeeded job only for specified number of seconds.")]
		public int KeepSeconds { get; set; }

		///
		protected override void BeginProcessing()
		{
			if (Hidden)
			{
				Output = Return = false;
				KeepSeconds = 0;
			}
			else if (Return)
			{
				Output = true;
				KeepSeconds = int.MaxValue;
			}
			else if (Output)
			{
				KeepSeconds = int.MaxValue;
			}

			// new
			Job job = new Job(
				Command,
				Parameters == null ? null : Parameters.BaseObject,
				Name,
				!Hidden && !Output,
				KeepSeconds);

			// start
			if (!Return)
				job.StartJob();

			// write
			if (Output)
				WriteObject(job);
		}
	}
}
