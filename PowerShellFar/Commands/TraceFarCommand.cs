/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System.ComponentModel;
using System.Diagnostics;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// Trace-Far command.
	/// Writes messages to the trace listeners.
	/// </summary>
	/// <remarks>
	/// See <c>[System.Diagnostics.Trace]</c> for more tracing tools.
	/// </remarks>
	[Description("Writes messages to the trace listeners.")]
	[CmdletBinding(DefaultParameterSetName = "Trace")]
	public sealed class TraceFarCommand : BaseCmdlet
	{
		///
		[Parameter(ParameterSetName = "Trace", Position = 0, Mandatory = true, ValueFromPipeline = true, HelpMessage = "A message to write.")]
		[AllowEmptyString]
		public string Message { get; set; }

		///
		[Parameter(ParameterSetName = "Trace", Position = 1, HelpMessage = "A category name used to organize the output.")]
		[AllowEmptyString]
		public string Category { get; set; }

		///
		[Parameter(ParameterSetName = "Trace", HelpMessage = "Writes error message.")]
		public SwitchParameter Error { get; set; }

		///
		[Parameter(ParameterSetName = "Trace", HelpMessage = "Writes warning message.")]
		public SwitchParameter Warning { get; set; }

		///
		[Parameter(ParameterSetName = "Trace", HelpMessage = "Writes information message.")]
		public SwitchParameter Information { get; set; }

		///
		[Parameter(ParameterSetName = "Event", Position = 0, Mandatory = true, HelpMessage = "Event ID.")]
		public int Id { get; set; }

		///
		[Parameter(ParameterSetName = "Event", Position = 1, Mandatory = true, HelpMessage = "Event type of the trace data.")]
		public TraceEventType EventType { get; set; }

		///
		[Parameter(ParameterSetName = "Event", Position = 2, HelpMessage = "Event message or format string with -Data.")]
		[AllowEmptyString]
		public string Format { get; set; }

		///
		[Parameter(ParameterSetName = "Event", Position = 3, HelpMessage = "Data for TraceEvent() or TraceData().")]
		public object[] Data { get; set; }

		///
		protected override void ProcessRecord()
		{
			if (EventType != 0)
			{
				if (Format == null)
					Log.Source.TraceData(EventType, Id, Data);
				else if (Data == null)
					Log.Source.TraceEvent(EventType, Id, Format);
				else
					Log.Source.TraceEvent(EventType, Id, Format, Data);
			}
			else if (Error)
			{
				Trace.TraceError(Message);
			}
			else if (Warning)
			{
				Trace.TraceWarning(Message);
			}
			else if (Information)
			{
				Trace.TraceInformation(Message);
			}
			else
			{
				if (Category == null)
					Trace.WriteLine(Message);
				else
					Trace.WriteLine(Message, Category);
			}
		}

	}
}
