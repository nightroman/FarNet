/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace PowerShellFar
{
	/// <summary>
	/// Base of PSF UI.
	/// </summary>
	/// <remarks>
	/// Basically most of writing methods are called in here,
	/// then they recall new virtual methods Append*.
	/// </remarks>
	abstract class UniformUI : PSHostUserInterface
	{
		/// <summary>
		/// UI output modes
		/// </summary>
		internal enum WriteMode
		{
			None,
			Debug,
			Error,
			Verbose,
			Warning
		};

		// Current mode.
		WriteMode _mode;
		internal void SetMode(WriteMode value)
		{
			_mode = value;
		}

		internal UniformUI()
		{
		}

		/// <summary>
		/// 1st of 3 actual writers.
		/// </summary>
		internal abstract void Append(string value);

		/// <summary>
		/// 2nd of 3 actual writers.
		/// </summary>
		internal abstract void AppendLine();

		/// <summary>
		/// 3rd of 3 actual writers.
		/// </summary>
		internal abstract void AppendLine(string value);

		#region PSHostUserInterface

		public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
		{
			throw new NotImplementedException();
		}

		public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
		{
			throw new NotImplementedException();
		}

		public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
		{
			throw new NotImplementedException();
		}

		public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
		{
			return NativeMethods.PromptForCredential(caption, message, userName, targetName, allowedCredentialTypes, options);
		}

		public override PSHostRawUserInterface RawUI
		{
			get { return new RawUI(); }
		}

		public override string ReadLine()
		{
			throw new NotImplementedException();
		}

		public override System.Security.SecureString ReadLineAsSecureString()
		{
			throw new NotImplementedException();
		}

		public override void WriteProgress(long sourceId, ProgressRecord record)
		{
		}

		public sealed override void Write(string value)
		{
			_mode = WriteMode.None;
			Append(value);
		}

		public sealed override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
		{
			_mode = WriteMode.None;
			Append(value);
		}

		public sealed override void WriteDebugLine(string message)
		{
			if (_mode != WriteMode.Debug)
			{
				_mode = WriteMode.Debug;
				AppendLine("DEBUG:");
			}
			AppendLine(message);
		}

		public override void WriteErrorLine(string value)
		{
			if (_mode != WriteMode.Error)
			{
				_mode = WriteMode.Error;
				AppendLine("ERROR:");
			}
			AppendLine(value);
		}

		public sealed override void WriteLine()
		{
			_mode = WriteMode.None;
			AppendLine();
		}

		public sealed override void WriteLine(string value)
		{
			_mode = WriteMode.None;
			AppendLine(value);
		}

		public sealed override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
		{
			_mode = WriteMode.None;
			AppendLine(value);
		}

		public sealed override void WriteVerboseLine(string message)
		{
			if (_mode != WriteMode.Verbose)
			{
				_mode = WriteMode.Verbose;
				AppendLine("VERBOSE:");
			}
			AppendLine(message);
		}

		public sealed override void WriteWarningLine(string message)
		{
			if (_mode != WriteMode.Warning)
			{
				_mode = WriteMode.Warning;
				AppendLine("WARNING:");
			}
			AppendLine(message);
		}

		#endregion
	}
}
