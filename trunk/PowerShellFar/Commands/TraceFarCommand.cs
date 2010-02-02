/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System.ComponentModel;
using System.Diagnostics;
using System.Management.Automation;

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
	public sealed class TraceFarCommand : BaseCmdlet
	{
		///
		[Parameter(Position = 0, ValueFromPipeline = true, ParameterSetName = "Message", HelpMessage = "A message to write.")]
		[AllowEmptyString]
		public string Message
		{
			get { return _Message; }
			set { _Message = value; }
		}
		string _Message;

		///
		[Parameter(Position = 1, HelpMessage = "A category name used to organize the output.")]
		[AllowEmptyString]
		public string Category
		{
			get { return _Category; }
			set { _Category = value; }
		}
		string _Category;

		///
		[Parameter(HelpMessage = "Writes error message.")]
		public SwitchParameter Error
		{
			get { return _Error; }
			set { _Error = value; }
		}
		SwitchParameter _Error;

		///
		[Parameter(HelpMessage = "Writes warning message.")]
		public SwitchParameter Warning
		{
			get { return _Warning; }
			set { _Warning = value; }
		}
		SwitchParameter _Warning;

		///
		[Parameter(HelpMessage = "Writes information message.")]
		public SwitchParameter Information
		{
			get { return _Information; }
			set { _Information = value; }
		}
		SwitchParameter _Information;

		///
		protected override void ProcessRecord()
		{
			Process();
		}

		[ConditionalAttribute("TRACE")]
		void Process()
		{
			if (_Error)
			{
				Trace.TraceError(_Message);
			}
			else if (_Warning)
			{
				Trace.TraceWarning(_Message);
			}
			else if (_Information)
			{
				Trace.TraceInformation(_Message);
			}
			else
			{
				if (_Category == null)
					Trace.WriteLine(_Message);
				else
					Trace.WriteLine(_Message, _Category);
			}
		}

	}
}
